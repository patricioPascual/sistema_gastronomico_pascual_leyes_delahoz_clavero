using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El Nombre es Obligatorio")]
        public String? Nombre { get; set; }

        [Column("cantidad_stock")]
        [Required(ErrorMessage = "La Cantidad de Stock es Obligatoria")]
        public decimal Cantidad_stock { get; set; }

        [Column("unidad_medida")]
        [Required(ErrorMessage = "La Unidad de Medida es Obligatoria")]
        public String? Unidad_medida { get; set; }

        [Column("precio_costo")]
        [Required(ErrorMessage = "El Precio es Obligatorio")]
        public decimal Precio_costo { get; set; }

        [Column("estado")]
        [Required(ErrorMessage = "Campo obligatorio")]
        public bool Estado { get; set; }
    }

}