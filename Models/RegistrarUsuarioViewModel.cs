using System.ComponentModel.DataAnnotations;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    // Combina los datos de Empleado + Usuario: el admin registra ambos en un solo formulario.
    // Las reglas salen de ReglasValidacion para que coincidan con Empleado y EditarUsuarioViewModel.
    public class RegistrarUsuarioViewModel
    {
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

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [DataType(DataType.Date)]
        [FechaNoFutura(ReglasValidacion.FechaMinimaIngreso, soloFecha: true,
            ErrorMessage = "La fecha de ingreso debe estar entre el 01/01/1990 y hoy")]
        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Seleccioná un rol")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un rol")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [RegularExpression(ReglasValidacion.RegexUsername, ErrorMessage = ReglasValidacion.MensajeUsername)]
        [Display(Name = "Usuario")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [RegularExpression(ReglasValidacion.RegexPassword, ErrorMessage = ReglasValidacion.MensajePassword)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirmá la contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string? ConfirmPassword { get; set; }

        public List<Rol> RolesDisponibles { get; set; } = new();
    }
}
