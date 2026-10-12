using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class EditarUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El legajo es obligatorio")]
        public string? Legajo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string? Dni { get; set; }

        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccioná un rol")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string? Username { get; set; }

        [Display(Name = "Nueva contraseña")]
        [DataType(DataType.Password)]
        public string? NuevaPassword { get; set; }

        [Display(Name = "Confirmar nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmarNuevaPassword { get; set; }

        public List<Rol> RolesDisponibles { get; set; } = new();
    }
}