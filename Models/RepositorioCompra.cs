using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioCompra 
    {
        private readonly GastronomiaContext _context;
                public RepositorioCompra(GastronomiaContext context)
        {
            _context = context;
        }

    public int Alta(Compra compra)
        {
            // Usamos una transacción para garantizar que si falla la actualización del stock, no se guarde la compra
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                compra.FechaHora = compra.FechaHora == default ? DateTime.Now : compra.FechaHora;
                compra.Estado = true;

                // 1. Recalcular total por seguridad
                if (compra.Detalles != null && compra.Detalles.Count > 0)
                {
                    compra.TotalCompra = compra.Detalles.Sum(d => d.CantidadIngresada * d.PrecioCostoUnitario);

                    // 2. Impactar el stock y precio de costo en cada Producto
                    foreach (var detalle in compra.Detalles)
                    {
                        var producto = _context.Productos.Find(detalle.IdProducto);
                        if (producto != null)
                        {
                            producto.Cantidad_stock += detalle.CantidadIngresada;
                            producto.Precio_costo = detalle.PrecioCostoUnitario; // Actualiza al último precio ingresado
                            _context.Productos.Update(producto);
                        }
                    }
                }

                // 3. Insertar la compra (EF Core inserta la cabecera y los Detalles vinculados automáticamente)
                _context.Compras.Add(compra);
                _context.SaveChanges();

                transaction.Commit();
                return compra.IdCompra; // EF Core actualiza IdCompra automáticamente con el id generado
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    

        
public List<Compra> ObtenerLista(int pagNro = 1, int tamPagina = 10)
        {
            int offset = (pagNro - 1) * tamPagina;

            return _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Where(c => c.Estado) // Solo las activas
                .OrderByDescending(c => c.FechaHora)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

    
        public Compra? ObtenerPorId(int id)
        {
            return _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto) // Trae los datos del producto de cada detalle
                .FirstOrDefault(c => c.IdCompra == id && c.Estado);
        }

    
        public bool Baja(int id)
        {
            var compra = ObtenerPorId(id);
            if (compra == null || !compra.Estado) return false;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. Revertir el stock en la tabla de productos
                if (compra.Detalles != null)
                {
                    foreach (var detalle in compra.Detalles)
                    {
                        var producto = _context.Productos.Find(detalle.IdProducto);
                        if (producto != null)
                        {
                            producto.Cantidad_stock -= detalle.CantidadIngresada;
                            if (producto.Cantidad_stock < 0) producto.Cantidad_stock = 0; 
                            _context.Productos.Update(producto);
                        }
                    }
                }

                
                compra.Estado = false;
                _context.Compras.Update(compra);

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

        public bool Modificar(Compra compraModificada)
{
    using var transaction = _context.Database.BeginTransaction();
    try
    {
        
        var compraOriginal = _context.Compras
            .Include(c => c.Detalles)
            .FirstOrDefault(c => c.IdCompra == compraModificada.IdCompra && c.Estado);

        if (compraOriginal == null) return false;

        //  Revertir EL Stock la compra original
        if (compraOriginal.Detalles != null)
        {
            foreach (var detalleViejo in compraOriginal.Detalles)
            {
                var producto = _context.Productos.Find(detalleViejo.IdProducto);
                if (producto != null)
                {
                    producto.Cantidad_stock -= detalleViejo.CantidadIngresada;
                    if (producto.Cantidad_stock < 0) producto.Cantidad_stock = 0;
                    _context.Productos.Update(producto);
                }
            }
        }

        //Actualizar la compra
        compraOriginal.IdProveedor = compraModificada.IdProveedor;
        compraOriginal.NumeroComprobante = compraModificada.NumeroComprobante;
        compraOriginal.FechaHora = compraModificada.FechaHora != default ? compraModificada.FechaHora : compraOriginal.FechaHora;

        // Eliminar los detalles anteriores de la BD
        if (compraOriginal.Detalles != null && compraOriginal.Detalles.Any())
        {
            _context.DetalleCompras.RemoveRange(compraOriginal.Detalles);
        }

        //  Agregar los nuevos detalles y aplicar el nuevo stock
        compraOriginal.Detalles = new List<DetalleCompra>();
        decimal totalCalculado = 0;

        if (compraModificada.Detalles != null)
        {
            foreach (var nuevoDetalle in compraModificada.Detalles)
            {
            
                nuevoDetalle.IdCompra = compraOriginal.IdCompra;
                compraOriginal.Detalles.Add(nuevoDetalle);

                totalCalculado += nuevoDetalle.CantidadIngresada * nuevoDetalle.PrecioCostoUnitario;

                var producto = _context.Productos.Find(nuevoDetalle.IdProducto);
                if (producto != null)
                {
                    producto.Cantidad_stock += nuevoDetalle.CantidadIngresada;
                    producto.Precio_costo = nuevoDetalle.PrecioCostoUnitario;
                    _context.Productos.Update(producto);
                }
            }
        }

        compraOriginal.TotalCompra = totalCalculado;

        _context.Compras.Update(compraOriginal);
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
    }
}



    

    

