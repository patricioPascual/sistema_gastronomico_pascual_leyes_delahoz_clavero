using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Rol
    {
        [Key]
        [Display(Name = "Codigo")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "Se requiere un nombre")]
        public String? Nombre { get; set; }
    }
}