using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        public enum Estado
        {
            Abierto,
            Pagado,
            Cancelado
        }

        [Column("estado")]
        public Estado estado { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        [Required]
        [Column("id_mesa")]
        public int IdMesa { get; set; }

        [ForeignKey(nameof(IdMesa))]
        public Mesa? Mesa { get; set; }

        [Required]
        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [ForeignKey(nameof(IdEmpleado))]
        public Empleado? Empleado { get; set; }

        // Propiedad de navegación
        public List<DetallePedido> Detalles { get; set; } = new();
    }
}