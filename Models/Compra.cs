using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int IdProveedor { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public Proveedor? Proveedor { get; set; }

        public string? NumeroComprobante { get; set; }

        [Required]
        public decimal TotalCompra { get; set; }

        [Required]
        public int IdEmpleado { get; set; }

        [ForeignKey(nameof(IdEmpleado))]
        public Empleado? Empleado { get; set; }

        public bool Estado {get;set;}

        // Propiedad de navegación para acceder a los insumos comprados
        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}