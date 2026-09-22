using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class Producto
    {
        [Key]
        [Display(Name="Codigo")]
        public int IdProducto {get;set;}

        [Required(ErrorMessage ="Se Requiere un nombre")]
        public String? Nombre{get;set;}

        public decimal Cantidad_stock{get;set;}

        [Required(ErrorMessage ="La unidad de medida es obligatoria")]
        public String? Unidad_medida {get;set;}

        [Required(ErrorMessage ="El precio es obligatorio")]
        public decimal Precio_costo {get;set;}

        public bool Estado {get;set;}
    }

}