using System; 
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero
{
    
    public class Mesa
    {
        
    [Key]
    public int IdMesa{get;set;}

    [Required(ErrorMessage ="El Numero de mesa es Obligatorio")]
    public int Numero {get;set;}


    public int Capacidad {get;set;}

    public bool Estado {get;set;}    

    }
}