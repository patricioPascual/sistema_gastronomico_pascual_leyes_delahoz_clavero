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
        public String? Nombre { get; set; }
        [Column("cantidad_stock")]
        public decimal Cantidad_stock { get; set; }
        [Column("unidad_medida")]
        public String? Unidad_medida { get; set; }
        [Column("precio_costo")]
        public decimal Precio_costo { get; set; }
        [Column("estado")]
        public bool Estado { get; set; }
    }

}