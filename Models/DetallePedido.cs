using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{

    public class DetallePedido
    {

        [Key]
        public int IdDetallePedido { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public enum Estado
        {
            EnMarcha,
            Despachado
        }
        public Estado estado { get; set; }

        [Required]
        [ForeignKey(nameof(Pedido))]
        public int IdPedido { get; set; }
        public Pedido? Pedido { get; set; }


        [Required]
        [ForeignKey(nameof(Plato))]
        public int IdPlato { get; set; }
        public Plato? Plato { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
