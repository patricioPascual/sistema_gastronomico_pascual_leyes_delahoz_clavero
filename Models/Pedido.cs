using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero
{

    public class Pedido
    {

        [Key]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "La Fecha y Hora son Obligatorias")]
        public DateTime FechaHora { get; set; }

        [Required]
        public Boolean Estado { get; set; }

        public int Total { get; set; }

        [Required]
        [ForeignKey(nameof(Mesa))]
        public int IdMesa { get; set; }
        public Mesa? Mesa { get; set; }
        

        [Required]
        [ForeignKey(nameof(Empleado))]
        public int IdEmpleado { get; set; }
        public Empleado? Empleado { get; set; }
    }
}
