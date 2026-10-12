using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    // Combina los datos de Empleado + Usuario: el admin registra ambos en un solo formulario. Esto no se si lo tendriamos que mantener.
    public class RegistrarUsuarioViewModel
    {
        [Required(ErrorMessage = "El legajo es obligatorio")]
        [Display(Name = "Legajo")]
        public string? Legajo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [Display(Name = "DNI")]
        public string? Dni { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Today;


        [Required(ErrorMessage = "Seleccioná un rol")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
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
