using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("proveedor")] // Asegura el nombre exacto de la tabla en la BD
    public class Proveedor
    {
        [Key]
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.&'\-]+$", ErrorMessage = "El nombre contiene caracteres no válidos")]
        [Column("nombre")]
        public string? Nombre { get; set; }

        // El formato lo controla la regex (sirve también del lado del cliente);
        // el dígito verificador lo controla [CuitValido] en el servidor.
        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [RegularExpression(ReglasValidacion.RegexCuit, ErrorMessage = "El CUIT debe tener el formato XX-XXXXXXXX-X")]
        [CuitValido]
        [Column("cuit")]
        public string? Cuit { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres")]
        [RegularExpression(ReglasValidacion.RegexTelefono, ErrorMessage = "Ingrese un número de teléfono válido")]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [StringLength(150, ErrorMessage = "La dirección no puede superar los 150 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,°ª'/\-]+$", ErrorMessage = "La dirección contiene caracteres no válidos")]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        // Propiedad de navegación inversa: Un proveedor puede tener muchas compras asociadas
        public List<Compra> Compras { get; set; } = new();
    }
}
