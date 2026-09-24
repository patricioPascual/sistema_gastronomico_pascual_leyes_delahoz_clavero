using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Plato
    {
        [Key]
        public int IdPlato { get; set; }

<<<<<<< HEAD
        [Required(ErrorMessage = "El Nombre es Obligatorio")]
        public string? Nombre { get; set; }

        public decimal PrecioVenta { get; set; }
        public bool Estado { get; set; }
        public int IdCategoria { get; set; }
        public Categoria? TipoPlato { get; set; }
=======
    [Required(ErrorMessage ="El Nombre del es Obligatorio")]
    public string Nombre {get;set;}
    public decimal PrecioVenta {get;set;}
    public bool Estado {get;set;}
    public int IdCategoria { get; set;}
    public Categoria? TipoPlato { get; set;}
>>>>>>> 25bab16389b3fa5d1846e5576ae52922cb3fbcf9
    }
}
