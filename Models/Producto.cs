using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace sistema_gastronomico_pascual_leyes_delahoz_clavero
{
    public class Producto
    {
        [Key]
        [Display(Name="Codigo")]
        public int idProducto {get;set;}

        [Required(ErrorMessage ="Se Requiere un nombre")]
        public String? nombre{get;set;}

        public decimal cantidad_stock{get;set;}

        [Required(ErrorMessage ="La unidad de medida es obligatoria")]
        public String? unidad_medida {get;set;}

        [Required(ErrorMessage ="E precio es obligatorio")]
        public decimal precio_costo {get;set;}

        public bool estado {get;set;}
    }

}