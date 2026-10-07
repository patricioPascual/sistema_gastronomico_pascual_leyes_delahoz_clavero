using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("rol")]
    public class Rol
    {
        [Key]
        [Display(Name = "Codigo")]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "Se requiere un nombre")]
        [Column("nombre")]
        public String? Nombre { get; set; }
    }
}