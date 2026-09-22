using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero
{
    public class Usuario
    {
        [Key]
        [Display(Name = "Codigo")]
        public int IdUsuario { get; set; }

        [Display(Name = "Nº Empleado")]
        public int IdEmpleado { get; set; }

        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "Se requiere un nombre")]
        public String? Username { get; set; }

        [Display(Name = "Contraseña")]
        public String? PasswordHash { get; set; }

        public Boolean Estado { get; set; }

    }
}
