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
        var plato = _context.Platos.FirstOrDefault(p => p.IdPlato == detalle.IdPlato && p.Estado);
        if (plato == null)
        {
            throw new InvalidOperationException("El plato seleccionado no existe o no está disponible.");
        }

        // 1. VALIDACIÓN DE STOCK
        var infoStock = ObtenerStockDisponiblePlato(detalle.IdPlato);
        if (detalle.Cantidad > infoStock.CantidadDisponible)
        {
            string mensajeInsumo = !string.IsNullOrEmpty(infoStock.InsumoFaltante)
                ? $" (Falta: {infoStock.InsumoFaltante})"
                : "";
            throw new InvalidOperationException($"No hay stock suficiente de '{plato.Nombre}'{mensajeInsumo}. Quedan {infoStock.CantidadDisponible} porciones.");
        }

        detalle.PrecioUnitario = plato.PrecioVenta;
        detalle.estado = DetallePedido.Estado.EnMarcha;
        detalle.FechaHora = DateTime.Now;

        _context.DetallePedidos.Add(detalle);

        // 2. Descontar insumos
        DescontarStock(detalle.IdPlato, detalle.Cantidad);

        _context.SaveChanges();


        RecalcularTotalPedido(detalle.IdPedido);

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        throw; // Re-lanza la excepción con el mensaje de error
    }
}
      
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

      
        public bool CambiarEstadoDetalle(int idDetallePedido, DetallePedido.Estado nuevoEstado)
        {
            var detalle = _context.DetallePedidos.Find(idDetallePedido);
            if (detalle == null) return false;

            detalle.estado = nuevoEstado;
            _context.DetallePedidos.Update(detalle);
            return _context.SaveChanges() > 0;
        }


        public DetallePedido? ObtenerPorId(int idDetallePedido)
        {
            return _context.DetallePedidos
                .Include(d => d.Plato)
                .FirstOrDefault(d => d.IdDetallePedido == idDetallePedido);
        }


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
                 
        public ResultadoStockPlato ObtenerStockDisponiblePlato(int idPlato)  //TIPO agregado en DTOs
{
    var receta = _context.DetalleRecetas
        .Include(dr => dr.Producto)
        .Where(dr => dr.IdPlato == idPlato)
        .ToList();
  

    int maxPorcionesPosibles = int.MaxValue;
    string? insumoCritico = null;
    decimal stockActualCritico = 0;
    decimal requerimientoCritico = 0;

    foreach (var ingrediente in receta)
    {
        if (ingrediente.Producto == null || ingrediente.CantidadRequerida <= 0) 
            continue;

        // Cuántas porciones completas alcanzan con este insumo específico
        int porcionesParaEsteInsumo = (int)Math.Floor(ingrediente.Producto.Cantidad_stock / ingrediente.CantidadRequerida);

        // Si este ingrediente limita más la producción que los anteriores, lo marcamos como el cuello de botella
        if (porcionesParaEsteInsumo < maxPorcionesPosibles)
        {
            maxPorcionesPosibles = porcionesParaEsteInsumo;
            insumoCritico = ingrediente.Producto.Nombre;
            stockActualCritico = ingrediente.Producto.Cantidad_stock;
            requerimientoCritico = ingrediente.CantidadRequerida;
        }
    }

    return new ResultadoStockPlato
    {
        CantidadDisponible = Math.Max(0, maxPorcionesPosibles),
        InsumoFaltante = insumoCritico,
        StockActualInsumo = stockActualCritico,
        InsumoRequeridoPorPorcion = requerimientoCritico
    };
}
    }
}