using System; 
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero
{
    
    public class Plato
    {
        
    [Key]
    public int IdPlato{get;set;}

    [Required(ErrorMessage ="El Nombre del es Obligatorio")]
    public int Nombre {get;set;}
    public decimal PrecioVenta {get;set;}
    public bool Estado {get;set;}
    public int IdCategoria { get; set;}
    public Categoria? TipoPlato { get; set;}
    }
}