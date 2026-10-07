using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Display(Name = "Codigo")]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Display(Name = "Nº Empleado")]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [ForeignKey("IdEmpleado")]
        public Empleado? Empleado { get; set; }

        [Display(Name = "Rol")]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [ForeignKey("IdRol")]
        public Rol? Rol { get; set; }

        [Required(ErrorMessage = "Se requiere un nombre")]
        [Column("username")]
        public String? Username { get; set; }

        [Display(Name = "Contraseña")]
        [Column("password_hash")]
        public String? PasswordHash { get; set; }

        [Column("activo")]
        public Boolean Estado { get; set; }
    }
}