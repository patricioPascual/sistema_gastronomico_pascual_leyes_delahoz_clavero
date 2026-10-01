using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("compra")]
    public class Compra
    {
        [Key]
        [Column("id_compra")]
        public int IdCompra { get; set; }
        
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public Proveedor? Proveedor { get; set; }
        
        [Column("numero_comprobante")]
        public string? NumeroComprobante { get; set; }

        [Required]
        [Column("total_compra")]
        public decimal TotalCompra { get; set; }

        [Required]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [ForeignKey(nameof(IdEmpleado))]
        public Empleado? Empleado { get; set; }

        [Column("estado")]
        public bool Estado { get; set; }

        // Propiedad de navegación para los detalles de la compra
        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}