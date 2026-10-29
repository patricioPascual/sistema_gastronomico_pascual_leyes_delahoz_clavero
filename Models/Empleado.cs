using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("empleado")]
    public class Empleado
    {
        [Key]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El legajo es obligatorio")]
        [StringLength(20, ErrorMessage = "El legajo no puede superar los 20 caracteres")]
        [RegularExpression(ReglasValidacion.RegexLegajo, ErrorMessage = ReglasValidacion.MensajeLegajo)]
        [Column("legajo")]
        public string? Legajo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(ReglasValidacion.RegexNombrePersona, ErrorMessage = ReglasValidacion.MensajeNombrePersona)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [RegularExpression(ReglasValidacion.RegexNombrePersona, ErrorMessage = ReglasValidacion.MensajeNombrePersona)]
        [Column("apellido")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(20)]
        [RegularExpression(ReglasValidacion.RegexDni, ErrorMessage = ReglasValidacion.MensajeDni)]
        [Column("dni")]
        public string? Dni { get; set; }

        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        [RegularExpression(ReglasValidacion.RegexTelefono, ErrorMessage = ReglasValidacion.MensajeTelefono)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [DataType(DataType.Date)]
        [FechaNoFutura(ReglasValidacion.FechaMinimaIngreso, soloFecha: true,
            ErrorMessage = "La fecha de ingreso debe estar entre el 01/01/1990 y hoy")]
        [Column("fecha_ingreso")]
        public DateTime Fecha_ingreso { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }
    }
}
