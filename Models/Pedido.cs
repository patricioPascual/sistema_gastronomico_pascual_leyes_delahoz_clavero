using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{

  public class Pedido
  {

    [Key]
    public int IdPedido { get; set; }

    public DateTime FechaHora { get; set; }

    public enum Estado
    {
      Abierto, Pagado, Cancelado
    }
    public Estado estado { get; set; }

    public decimal Total { get; set; }

    [Required]
    //  [ForeignKey(nameof(Mesa))]
    public int IdMesa { get; set; }
    public Mesa? Mesa { get; set; }


    [Required]
    //    [ForeignKey(nameof(Empleado))]
    public int IdEmpleado { get; set; }
    public Empleado? Empleado { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
  }
}
