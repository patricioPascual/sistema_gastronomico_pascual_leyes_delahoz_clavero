using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Categoria
    {
        [Key]
        [Display(Name = "Codigo")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Se requiere un nombre")]
        public String? Nombre { get; set; }

        public Boolean Estado { get; set; }
    }
}