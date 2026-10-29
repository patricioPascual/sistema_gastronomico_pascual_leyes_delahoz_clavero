using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers
{
    // Arma el ValidationResult indicando a qué propiedad pertenece el error (MemberNames),
    // igual que hacen los atributos de .NET. Así el error queda asociado al campo
    // y se puede verificar en las pruebas unitarias.
    internal static class ResultadoValidacion
    {
        public static ValidationResult Error(string mensaje, ValidationContext contexto)
        {
            return contexto.MemberName is null
                ? new ValidationResult(mensaje)
                : new ValidationResult(mensaje, new[] { contexto.MemberName });
        }
    }

    // Fecha entre un mínimo fijo y "ahora" (más una tolerancia opcional en minutos).
    // Se usa en Fecha de ingreso del empleado y en Fecha/hora de la compra.
    // Es un atributo propio porque [Range] solo admite límites constantes y "hoy" cambia todos los días.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class FechaNoFuturaAttribute : ValidationAttribute
    {
        private readonly DateTime fechaMinima;
        private readonly int toleranciaMinutos;
        private readonly bool soloFecha;

        public FechaNoFuturaAttribute(string fechaMinima, int toleranciaMinutos = 0, bool soloFecha = false)
        {
            this.fechaMinima = DateTime.ParseExact(fechaMinima, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.toleranciaMinutos = toleranciaMinutos;
            this.soloFecha = soloFecha;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Si viene vacío, que lo resuelva [Required].
            if (value is not DateTime fecha)
                return ValidationResult.Success;

            DateTime maxima = soloFecha ? DateTime.Today : DateTime.Now.AddMinutes(toleranciaMinutos);
            DateTime aComparar = soloFecha ? fecha.Date : fecha;

            if (aComparar < fechaMinima)
                return ResultadoValidacion.Error(ErrorMessage ?? $"La fecha no puede ser anterior al {fechaMinima:dd/MM/yyyy}.", validationContext);

            if (aComparar > maxima)
                return ResultadoValidacion.Error(ErrorMessage ?? "La fecha no puede ser futura.", validationContext);

            return ValidationResult.Success;
        }
    }

    // CUIT con formato XX-XXXXXXXX-X y dígito verificador correcto (algoritmo módulo 11 de AFIP/ARCA).
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CuitValidoAttribute : ValidationAttribute
    {
        private static readonly int[] Pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var cuit = value as string;
            if (string.IsNullOrWhiteSpace(cuit))
                return ValidationResult.Success; // lo resuelve [Required]

            // El formato ya lo informa [RegularExpression] en el modelo; acá solo se evita
            // calcular el dígito sobre un texto mal formado (y duplicar el mensaje).
            if (!Regex.IsMatch(cuit, ReglasValidacion.RegexCuit))
                return ValidationResult.Success;

            if (!EsCuitValido(cuit))
                return ResultadoValidacion.Error("El CUIT no es válido: el dígito verificador no coincide.", validationContext);

            return ValidationResult.Success;
        }

        public static bool EsCuitValido(string cuit)
        {
            var digitos = cuit.Replace("-", "");
            if (digitos.Length != 11 || !digitos.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 10; i++)
                suma += (digitos[i] - '0') * Pesos[i];

            int verificador = 11 - (suma % 11);
            if (verificador == 11) verificador = 0;

            // Resultado 10: AFIP no emite ese CUIT con ese prefijo (cambia el prefijo a 23/24/33), así que es inválido.
            if (verificador == 10)
                return false;

            return verificador == digitos[10] - '0';
        }
    }

    // Unidad de medida perteneciente a la lista cerrada de UnidadesMedida.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UnidadMedidaValidaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var unidad = value as string;
            if (string.IsNullOrWhiteSpace(unidad))
                return ValidationResult.Success; // lo resuelve [Required]

            if (!UnidadesMedida.EsValida(unidad))
                return ResultadoValidacion.Error("Unidad de medida inválida. Elegí una de la lista: " + string.Join(", ", UnidadesMedida.Todas.Keys) + ".", validationContext);

            return ValidationResult.Success;
        }
    }
}
