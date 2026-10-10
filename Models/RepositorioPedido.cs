using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioPedido
    {
        private readonly GastronomiaContext _context;
        private readonly RepositorioDetallePedido _repositorioDetalle;

        public RepositorioPedido(GastronomiaContext context, RepositorioDetallePedido repositorioDetalle)
        {
            _context = context;
            _repositorioDetalle = repositorioDetalle;
        }

        public int Alta(Pedido pedido)
        {
            // 1. Validar el stock de TODOS los platos antes de abrir la transacción o guardar algo
            if (pedido.Detalles != null && pedido.Detalles.Count > 0)
            {
                foreach (var detalle in pedido.Detalles)
                {
                    var plato = _context.Platos.FirstOrDefault(p => p.IdPlato == detalle.IdPlato && p.Estado);
                    if (plato == null)
                    {
                        throw new InvalidOperationException("Uno de los platos seleccionados no existe o no está activo.");
                    }

                    var infoStock = _repositorioDetalle.ObtenerStockDisponiblePlato(detalle.IdPlato);
                    if (detalle.Cantidad > infoStock.CantidadDisponible)
                    {
                        string mensajeInsumo = !string.IsNullOrEmpty(infoStock.InsumoFaltante)
                            ? $" (Falta: {infoStock.InsumoFaltante})"
                            : "";
                        throw new InvalidOperationException($"Stock insuficiente para '{plato.Nombre}'{mensajeInsumo}. Quedan {infoStock.CantidadDisponible} porciones.");
                    }
                }
            }

            // 2. Si todos los platos tienen stock suficiente, procedemos con la transacción atómica
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                pedido.FechaHora = DateTime.Now;
                pedido.estado = Pedido.Estado.Abierto;

                if (pedido.Detalles != null && pedido.Detalles.Count > 0)
                {
                    foreach (var detalle in pedido.Detalles)
                    {
                        var plato = _context.Platos.Find(detalle.IdPlato);
                        detalle.PrecioUnitario = plato!.PrecioVenta;
                        detalle.estado = DetallePedido.Estado.EnMarcha;
                        detalle.FechaHora = DateTime.Now;

                        // Descontar stock de insumos
                        _repositorioDetalle.DescontarStock(detalle.IdPlato, detalle.Cantidad);
                    }

                    pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                }

                _context.Pedidos.Add(pedido);

                // Si esta mesa tenía un pedido pagado pendiente de liberar, el
                // pedido nuevo lo reemplaza: se apaga la bandera.
                var mesa = _context.Mesas.Find(pedido.IdMesa);
                if (mesa != null && mesa.Estado)
                {
                    mesa.Estado = false;
                    _context.Mesas.Update(mesa);
                }

                _context.SaveChanges();

                transaction.Commit();
                return pedido.IdPedido;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Baja(int idPedido)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefault(p => p.IdPedido == idPedido);

            if (pedido == null || pedido.estado == Pedido.Estado.Cancelado) return false;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (pedido.Detalles != null)
                {
                    foreach (var detalle in pedido.Detalles)
                    {
                        _repositorioDetalle.RestituirStock(detalle.IdPlato, detalle.Cantidad);
                    }
                }

                pedido.estado = Pedido.Estado.Cancelado;
                _context.Pedidos.Update(pedido);

                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Modificar(Pedido pedido)
        {
            var pedidoExistente = _context.Pedidos
                .FirstOrDefault(p => p.IdPedido == pedido.IdPedido && p.estado == Pedido.Estado.Abierto);

            if (pedidoExistente == null) return false;

            pedidoExistente.IdMesa = pedido.IdMesa;
            pedidoExistente.IdEmpleado = pedido.IdEmpleado;

            _context.Pedidos.Update(pedidoExistente);
            return _context.SaveChanges() > 0;
        }

        public List<Pedido> ObtenerLista(int pagNro = 1, int tamPagina = 10)
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .OrderByDescending(p => p.FechaHora)
                .Skip((pagNro - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();
        }

        public List<Pedido> ObtenerAbiertos()
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Where(p => p.estado == Pedido.Estado.Abierto)
                .OrderByDescending(p => p.FechaHora)
                .ToList();
        }

        public int ObtenerCantidad()
        {
            return _context.Pedidos.Count();
        }

        public Pedido? ObtenerPorId(int idPedido)
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                .FirstOrDefault(p => p.IdPedido == idPedido);
        }

        public bool CerrarYPagarPedido(int idPedido)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefault(p => p.IdPedido == idPedido);

            if (pedido == null || pedido.estado != Pedido.Estado.Abierto)
            {
                return false;
            }

            pedido.Total = pedido.Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;
            pedido.estado = Pedido.Estado.Pagado;
            _context.Pedidos.Update(pedido);

            var mesa = _context.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa);
            if (mesa != null)
            {
                // Queda pendiente de liberar: se cobró, pero la mesa sigue
                // marcada hasta que un mozo la libere o abra un pedido nuevo ahí.
                mesa.Estado = true;
                _context.Mesas.Update(mesa);
            }
            return _context.SaveChanges() > 0;
        }

        public List<Pedido> ObtenerPedidosConDetallesEnMarcha()
        {
            // Ya no exige pedido.estado == Abierto: un plato "En Marcha" se tiene
            // que seguir viendo en el salón aunque la mesa ya se haya cobrado y
            // cerrado (CerrarYPagarPedido no obliga a despachar todo antes de
            // cobrar). Solo se excluyen los pedidos Cancelados, que no tienen
            // nada pendiente de cocina.
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Detalles.Where(d => d.estado == DetallePedido.Estado.EnMarcha))
                    .ThenInclude(d => d.Plato)
                .Where(p => p.estado != Pedido.Estado.Cancelado && p.Detalles.Any(d => d.estado == DetallePedido.Estado.EnMarcha))
                .OrderBy(p => p.FechaHora)
                .ToList();
        }

        // Id del último pedido Pagado de esta mesa (el más reciente por fecha).
        // Se usa para saber si un pedido Pagado puntual es "el" pendiente de
        // liberar actual de la mesa, o si quedó viejo porque ya se abrió otro
        // pedido (y ese otro es el que manda ahora).
        public int? ObtenerUltimoPedidoPagadoId(int idMesa)
        {
            return _context.Pedidos
                .Where(p => p.IdMesa == idMesa && p.estado == Pedido.Estado.Pagado)
                .OrderByDescending(p => p.FechaHora)
                .Select(p => (int?)p.IdPedido)
                .FirstOrDefault();
        }

    }
}