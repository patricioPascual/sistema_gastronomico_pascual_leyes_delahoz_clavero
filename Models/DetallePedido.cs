using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [Column("id_detalle_pedido")]
        public int IdDetallePedido { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        public enum Estado
        {
            EnMarcha,
            Despachado
        }

        [Column("estado")]
        public Estado estado { get; set; }

       
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [ForeignKey(nameof(IdPedido))]
        public Pedido? Pedido { get; set; }

        [Required]
        [Column("id_plato")]
        public int IdPlato { get; set; }

        [ForeignKey(nameof(IdPlato))]
        public Plato? Plato { get; set; }

        [NotMapped] // Indico a EF Core que no cree una columna para Subtotal, ya que se calcula en memoria
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}