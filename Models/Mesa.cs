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

        [Column("estado")]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [Column("tipo")]
        public string Tipo { get; set; } = "Mesa";

        // Propiedad transitoria (calculada en memoria para el Mapa del Salón).
        // Se le indica a EF Core que no cree una columna física en la base de datos.
        [NotMapped]
        public int? IdPedidoAbierto { get; set; }
    }
}