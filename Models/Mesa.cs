using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("mesa")]
    public class Mesa
    {
        [Key]
        [Column("id_mesa")]
        public int IdMesa { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio")]
        [Column("numero")]
        public int Numero { get; set; }

        [Column("capacidad")]
        public int Capacidad { get; set; }

        // Ya NO es "legado sin uso": ahora significa "tiene un pedido pagado
        // pendiente de liberar" (true) o "sin pendientes" (false). Una mesa
        // nueva arranca en false. Se pone en true en CerrarYPagarPedido() y
        // vuelve a false al liberar la mesa o al abrir un pedido nuevo ahí.
        [Column("estado")]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [Column("tipo")]
        public string Tipo { get; set; } = "Mesa";

        // Propiedades transitorias (calculadas en memoria para el Mapa del Salón).
        // Se le indica a EF Core que no cree columna física para ninguna de las dos.
        [NotMapped]
        public int? IdPedidoAbierto { get; set; }

        // Id del último pedido Pagado de esta mesa, solo cuando Estado == true
        // (pendiente de liberar). Null si la mesa está libre u ocupada por un
        // pedido Abierto.
        [NotMapped]
        public int? IdPedidoPendienteLiberar { get; set; }
    }
}