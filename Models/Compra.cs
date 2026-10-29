using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("compra")]
    public class Compra
    {
        [Key]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        // Tolerancia de 5 minutos por diferencias de reloj entre el navegador y el servidor.
        [Display(Name = "Fecha")]
        [FechaNoFutura(ReglasValidacion.FechaMinimaCompra, toleranciaMinutos: 5,
            ErrorMessage = "La fecha de la compra no puede ser futura ni anterior al 01/01/2000")]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un proveedor")]
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public Proveedor? Proveedor { get; set; }

        // Opcional (la columna admite NULL). Si se carga, se valida formato y unicidad por proveedor
        // (la unicidad la controla CompraController contra uk_proveedor_comprobante).
        [Display(Name = "N° Comprobante")]
        [StringLength(50, ErrorMessage = "El comprobante no puede superar los 50 caracteres")]
        [RegularExpression(ReglasValidacion.RegexComprobante, ErrorMessage = ReglasValidacion.MensajeComprobante)]
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
