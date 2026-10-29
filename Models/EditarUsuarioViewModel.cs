using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class EditarUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El legajo es obligatorio")]
        [StringLength(20, ErrorMessage = "El legajo no puede superar los 20 caracteres")]
        [RegularExpression(ReglasValidacion.RegexLegajo, ErrorMessage = ReglasValidacion.MensajeLegajo)]
        [Display(Name = "Legajo")]
        public string? Legajo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(ReglasValidacion.RegexNombrePersona, ErrorMessage = ReglasValidacion.MensajeNombrePersona)]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [RegularExpression(ReglasValidacion.RegexNombrePersona, ErrorMessage = ReglasValidacion.MensajeNombrePersona)]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(ReglasValidacion.RegexDni, ErrorMessage = ReglasValidacion.MensajeDni)]
        [Display(Name = "DNI")]
        public string? Dni { get; set; }

        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        [RegularExpression(ReglasValidacion.RegexTelefono, ErrorMessage = ReglasValidacion.MensajeTelefono)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccioná un rol")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un rol")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [RegularExpression(ReglasValidacion.RegexUsername, ErrorMessage = ReglasValidacion.MensajeUsername)]
        [Display(Name = "Usuario")]
        public string? Username { get; set; }

        // Opcional: si viene vacía no se cambia la contraseña.
        // [RegularExpression] no valida null/vacío, así que la regla solo aplica cuando se escribe algo.
        [RegularExpression(ReglasValidacion.RegexPassword, ErrorMessage = ReglasValidacion.MensajePassword)]
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
