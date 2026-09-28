

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
   
   

    public class ComprasPorProveedorDto
    {
        public string NombreProveedor { get; set; } = string.Empty;
        public int CantidadCompras { get; set; }
        public decimal TotalAcumulado { get; set; }
    }

   
    public class PlatoMasVendidoDto
    {
        public string NombrePlato { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
    }

 
    public class BalanceGeneralDto
    {
        public decimal TotalVentas { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal SaldoNeto => TotalVentas - TotalCompras;
    }

    public class VentasPorMozoDto
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public int CantidadPedidos { get; set; }
    public decimal TotalVentas { get; set; }
}
}