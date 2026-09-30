using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("categoria")]
    public class Categoria
    {
        [Key]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }
        [Column("nombre")]
        public String? Nombre { get; set; }
        [Column("estado")]
        public Boolean Estado { get; set; }
    }
}