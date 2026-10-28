using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("plato")]
    public class Plato
    {
        [Key]
        [Column("id_plato")]
        public int IdPlato { get; set; }

        [Required(ErrorMessage = "El Nombre es Obligatorio")]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "El precio de venta debe estar entre 0,01 y 1.000.000")]
        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("activo")]
        public bool Estado { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("imagen")]
        public string? Imagen {get; set;}

        // con esto hacemos el JOIN
        [ForeignKey("IdCategoria")]
        public Categoria? Categoria { get; set; }

        // Relación 1 a muchos con los ingredientes de la receta
        public ICollection<DetalleReceta>? DetalleRecetas { get; set; }
    }
}