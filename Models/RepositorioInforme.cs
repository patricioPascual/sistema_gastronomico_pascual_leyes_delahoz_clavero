namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioInforme
    {
        private readonly GastronomiaContext _context;

        public RepositorioInforme(GastronomiaContext context)
        {
            _context = context;
        }

        // Solo cuentan como venta los pedidos cobrados
        private IQueryable<Pedido> PedidosPagados(DateTime desde, DateTime hasta)
        {
            return _context.Pedidos
                .Where(p => p.estado == Pedido.Estado.Pagado
                         && p.FechaHora >= desde
                         && p.FechaHora <= hasta);
        }

        private IQueryable<Compra> ComprasActivas(DateTime desde, DateTime hasta)
        {
            return _context.Compras
                .Where(c => c.Estado
                         && c.FechaHora >= desde
                         && c.FechaHora <= hasta);
        }

        public IList<PlatoMasVendidoDto> ObtenerPlatosMasVendidos(DateTime desde, DateTime hasta)
        {
            return _context.DetallePedidos
                .Where(dp => dp.Pedido!.estado == Pedido.Estado.Pagado
                          && dp.Pedido.FechaHora >= desde
                          && dp.Pedido.FechaHora <= hasta)
                .GroupBy(dp => new { dp.IdPlato, dp.Plato!.Nombre })
                .Select(g => new PlatoMasVendidoDto
                {
                    NombrePlato = g.Key.Nombre ?? string.Empty,
                    CantidadVendida = g.Sum(dp => dp.Cantidad),
                    TotalRecaudado = g.Sum(dp => dp.Cantidad * dp.PrecioUnitario)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(10)
                .ToList();
        }

        public IList<ComprasPorProveedorDto> ObtenerComprasPorProveedor(DateTime desde, DateTime hasta)
        {
            return ComprasActivas(desde, hasta)
                .GroupBy(c => new { c.IdProveedor, c.Proveedor!.Nombre })
                .Select(g => new ComprasPorProveedorDto
                {
                    NombreProveedor = g.Key.Nombre ?? string.Empty,
                    CantidadCompras = g.Count(),
                    TotalAcumulado = g.Sum(c => c.TotalCompra)
                })
                .OrderByDescending(x => x.TotalAcumulado)
                .ToList();
        }

        public BalanceGeneralDto ObtenerBalanceGeneral(DateTime desde, DateTime hasta)
        {
            // Sum devuelve 0 si no hay registros (reemplaza al COALESCE del SQL anterior)
            return new BalanceGeneralDto
            {
                TotalVentas = PedidosPagados(desde, hasta).Sum(p => p.Total),
                TotalCompras = ComprasActivas(desde, hasta).Sum(c => c.TotalCompra)
            };
        }

        public IList<VentasPorMozoDto> ObtenerVentasPorMozo(DateTime desde, DateTime hasta)
        {
            return PedidosPagados(desde, hasta)
                .GroupBy(p => new { p.IdEmpleado, p.Empleado!.Apellido, p.Empleado.Nombre })
                .Select(g => new VentasPorMozoDto
                {
                    NombreEmpleado = g.Key.Apellido + ", " + g.Key.Nombre,
                    CantidadPedidos = g.Count(),
                    TotalVentas = g.Sum(p => p.Total)
                })
                .OrderByDescending(x => x.TotalVentas)
                .ToList();
        }
    }
}
