using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Plato
    {
        [Key]
        public int IdPlato { get; set; }

        [Required(ErrorMessage = "El Nombre es Obligatorio")]
        public string? Nombre { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Estado { get; set; }
        public int IdCategoria { get; set; }
        public Categoria? TipoPlato { get; set; }
    }
}
