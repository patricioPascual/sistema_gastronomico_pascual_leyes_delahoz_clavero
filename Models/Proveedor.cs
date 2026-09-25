using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Proveedor
    {
        
     [Key]
     public int IdProveedor {get;set;}

     [Required(ErrorMessage ="EL nombre es Obligatorio")]
     [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.&'\-]+$", ErrorMessage = "El nombre contiene caracteres no válidos")]
     public String? Nombre {get;set;}

     [Required(ErrorMessage ="El CUIT es Obligatorio")]
     [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "El CUIT debe tener el formato XX-XXXXXXXX-X")]
     public String? Cuit{get;set;} 

     [Required(ErrorMessage ="El telefono es requerido")]
     [RegularExpression(@"^\+?[0-9\s\-\(\)]{6,20}$", ErrorMessage = "Ingrese un número de teléfono válido")]
     public String? Telefono {get;set;}

    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,°ª'/\-]+$", ErrorMessage = "La dirección contiene caracteres no válidos")]
     public String? Direccion {get;set;}

    public bool Estado {get;set;}

    }
   

}