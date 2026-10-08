using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioDetallePedido
    {
        private readonly GastronomiaContext _context;

        public RepositorioDetallePedido(GastronomiaContext context)
        {
            _context = context;
        }

        public bool Alta(DetallePedido detalle)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Obtener plato para asignar su precio actual de venta
                var plato = _context.Platos.FirstOrDefault(p => p.IdPlato == detalle.IdPlato && p.Estado);
                if (plato == null) return false;

                detalle.PrecioUnitario = plato.PrecioVenta;
                detalle.estado = DetallePedido.Estado.EnMarcha;
                detalle.FechaHora = DateTime.Now;

                _context.DetallePedidos.Add(detalle);

                // 2. Descontar el stock a nivel de detalle
                DescontarStock(detalle.IdPlato, detalle.Cantidad);

                _context.SaveChanges();

                // 3. Recalcular el total en la cabecera del Pedido
                RecalcularTotalPedido(detalle.IdPedido);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Elimina un detalle, restituye la cantidad al stock del producto y actualiza el total del pedido.
        /// </summary>
        public bool Baja(int idDetallePedido)
        {
            var detalle = _context.DetallePedidos.Find(idDetallePedido);
            if (detalle == null) return false;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int idPedido = detalle.IdPedido;

                // 1. Devolver la cantidad consumida al stock
                RestituirStock(detalle.IdPlato, detalle.Cantidad);

                // 2. Remover el detalle
                _context.DetallePedidos.Remove(detalle);
                _context.SaveChanges();

                // 3. Recalcular el total del pedido
                RecalcularTotalPedido(idPedido);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Modifica la cantidad de un ítem, ajustando únicamente la diferencia en stock.
        /// </summary>
        public bool ModificarCantidad(int idDetallePedido, int nuevaCantidad)
        {
            if (nuevaCantidad <= 0) return Baja(idDetallePedido);

            var detalle = _context.DetallePedidos.Find(idDetallePedido);
            if (detalle == null) return false;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int diferencia = nuevaCantidad - detalle.Cantidad;

                // Si la diferencia es positiva, se vendieron más unidades (descuenta stock).
                // Si la diferencia es negativa, se quitaron unidades (restituye stock).
                if (diferencia > 0)
                {
                    DescontarStock(detalle.IdPlato, diferencia);
                }
                else if (diferencia < 0)
                {
                    RestituirStock(detalle.IdPlato, Math.Abs(diferencia));
                }

                detalle.Cantidad = nuevaCantidad;
                _context.DetallePedidos.Update(detalle);
                _context.SaveChanges();

                RecalcularTotalPedido(detalle.IdPedido);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Cambia el estado del detalle (ej: de 'EnMarcha' a 'Despachado' para la cocina).
        /// </summary>
        public bool CambiarEstadoDetalle(int idDetallePedido, DetallePedido.Estado nuevoEstado)
        {
            var detalle = _context.DetallePedidos.Find(idDetallePedido);
            if (detalle == null) return false;

            detalle.estado = nuevoEstado;
            _context.DetallePedidos.Update(detalle);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Obtiene un detalle con los datos cargados de su Plato.
        /// </summary>
        public DetallePedido? ObtenerPorId(int idDetallePedido)
        {
            return _context.DetallePedidos
                .Include(d => d.Plato)
                .FirstOrDefault(d => d.IdDetallePedido == idDetallePedido);
        }

        // --- MÉTODOS PRIVADOS AUXILIARES PARA MANEJO DE STOCK Y TOTALES ---

        public void DescontarStock(int idPlato, int cantidad)
        {
            var producto = _context.Productos.Find(idPlato);
            if (producto != null)
            {
                producto.Cantidad_stock -= cantidad;
                if (producto.Cantidad_stock < 0) producto.Cantidad_stock = 0;
                _context.Productos.Update(producto);
            }
        }

        public void RestituirStock(int idPlato, int cantidad)
        {
            var producto = _context.Productos.Find(idPlato);
            if (producto != null)
            {
                producto.Cantidad_stock += cantidad;
                _context.Productos.Update(producto);
            }
        }

        private void RecalcularTotalPedido(int idPedido)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefault(p => p.IdPedido == idPedido);

            if (pedido != null)
            {
                pedido.Total = pedido.Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;
                _context.Pedidos.Update(pedido);
                _context.SaveChanges();
            }
        }
    }
}