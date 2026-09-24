using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("detalle_compra")]
    public class DetalleCompra
    {
        [Key]
        [Column("id_detalle_compra")]
        public int IdDetalleCompra { get; set; }

        [Required]
        [Column("id_compra")]
        public int IdCompra { get; set; }

        [ForeignKey(nameof(IdCompra))]

        public Compra? Compra { get; set; }

        [Required]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [ForeignKey(nameof(IdProducto))]
    
        public Producto? Producto { get; set; }

        [Required]
        [Column("cantidad_ingresada")]
        public decimal CantidadIngresada { get; set; }

        [Required]
        [Column("precio_costo_unitario")]
        public decimal PrecioCostoUnitario { get; set; }

        // Propiedad calculada en C# para facilitar lecturas en Vistas/Controllers
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("subtotal")]
        public decimal Subtotal => CantidadIngresada * PrecioCostoUnitario;
    }
}