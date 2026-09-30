using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("detalle_receta")]
    public class DetalleReceta
    {
        [Key]
        [Column("id_detalle_receta")]
        public int IdDetalleReceta { get; set; }

        [Column("id_plato")]
        public int IdPlato { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad_requerida")]
        public decimal CantidadRequerida { get; set; }

        [ForeignKey("IdPlato")]
        public Plato? Plato { get; set; }

        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }
    }
}