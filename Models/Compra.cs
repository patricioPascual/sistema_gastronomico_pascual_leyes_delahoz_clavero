using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [Column("proveedor")]
        public string? Proveedor { get; set; }

        [Column("numero_comprobante")]
        public string? NumeroComprobante { get; set; }

        [Required]
        [Column("total_compra")]
        public decimal TotalCompra { get; set; }

        [Required]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        public int idEmpleado{get;set;}
        [ForeignKey(nameof(IdEmpleado))]
        public Empleado? Empleado { get; set; }

        // Propiedad de navegación para acceder a los insumos comprados
        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}