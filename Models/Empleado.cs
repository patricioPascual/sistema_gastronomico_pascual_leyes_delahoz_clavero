using System; 
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    
    public class Empleado
    {
        [Key]
        public int IdEmpleado{get;set;}

        [Required(ErrorMessage ="El nombre es Obligatorio")]
        public String? Nombre {get;set;}

        [Required(ErrorMessage ="El Apellido es obligatorio")]
        public String? Apellido {get;set;}

        public String? Dni{get;set;}

        public String? Telefono {get;set;} 

        public DateTime Fecha_ingreso {get;set;}

        public bool Activo {get;set;}
        

    }
}