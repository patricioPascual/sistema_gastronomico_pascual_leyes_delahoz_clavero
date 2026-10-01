using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    [Table("proveedor")] // Asegura el nombre exacto de la tabla en la BD
    public class Proveedor
    {
        [Key]
        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.&'\-]+$", ErrorMessage = "El nombre contiene caracteres no válidos")]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "El CUIT debe tener el formato XX-XXXXXXXX-X")]
        [Column("cuit")]
        public string? Cuit { get; set; } 

        [Required(ErrorMessage = "El teléfono es requerido")]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{6,20}$", ErrorMessage = "Ingrese un número de teléfono válido")]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,°ª'/\-]+$", ErrorMessage = "La dirección contiene caracteres no válidos")]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        // Propiedad de navegación inversa: Un proveedor puede tener muchas compras asociadas
        public List<Compra> Compras { get; set; } = new();
    }
}