using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class DetalleReceta
    {
        [Key]
        public int IdDetalleReceta { get; set; }

        public int IdPlato { get; set; }
        public Plato? Plato { get; set; }

        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        [Required(ErrorMessage = "La cantidad requerida es obligatoria")]
        public decimal CantidadRequerida { get; set; }
    }
}
