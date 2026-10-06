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
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                pedido.FechaHora = DateTime.Now;
                pedido.estado = Pedido.Estado.Abierto;

                if (pedido.Detalles != null && pedido.Detalles.Count > 0)
                {
                    // 1. Asignar precio y descontar stock por cada detalle utilizando la lógica del detalle
                    foreach (var detalle in pedido.Detalles)
                    {
                        var plato = _context.Platos.FirstOrDefault(p => p.IdPlato == detalle.IdPlato && p.Estado);
                        if (plato != null)
                        {
                            detalle.PrecioUnitario = plato.PrecioVenta;
                            detalle.estado = DetallePedido.Estado.EnMarcha;
                            detalle.FechaHora = DateTime.Now;
                        }

                        // Descontar stock a nivel de producto
                        _repositorioDetalle.DescontarStock(detalle.IdPlato, detalle.Cantidad);
                    }

                    // 2. Calcular el total acumulado
                    pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                }

                // 3. Guardar el pedido y sus detalles
                _context.Pedidos.Add(pedido);
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
                // Restituir el stock de cada detalle
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
        mesa.Estado = false;
        _context.Mesas.Update(mesa);
    }
    return _context.SaveChanges() > 0;
}
     
        public List<Pedido> ObtenerPedidosConDetallesEnMarcha()
        {
            return _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.Empleado)
                .Include(p => p.Detalles.Where(d => d.estado == DetallePedido.Estado.EnMarcha))
                    .ThenInclude(d => d.Plato)
                .Where(p => p.estado == Pedido.Estado.Abierto && p.Detalles.Any(d => d.estado == DetallePedido.Estado.EnMarcha))
                .OrderBy(p => p.FechaHora)
                .ToList();
        }
    }
}