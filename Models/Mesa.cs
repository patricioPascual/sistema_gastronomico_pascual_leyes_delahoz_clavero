using System;
using System.ComponentModel.DataAnnotations;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{

    public class Mesa
    {

        [Key]
        public int IdMesa { get; set; }

        [Required(ErrorMessage = "El Numero de mesa es Obligatorio")]
        public int Numero { get; set; }

        public int Capacidad { get; set; }

        public bool Estado { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Tipo { get; set; } = "Mesa";

        // Transiente: no es columna propia, se completa solo en ObtenerTodosConOcupacion()
        // para saber a qué pedido linkear desde el Salón. Alta/Modificar la ignoran.
        public int? IdPedidoAbierto { get; set; }

    }
}