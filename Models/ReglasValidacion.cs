namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    // Patrones y límites compartidos por modelos y ViewModels.
    // Están centralizados para que Empleado, RegistrarUsuarioViewModel y EditarUsuarioViewModel
    // validen exactamente lo mismo, y para que los largos coincidan con los varchar(n) de la base.
    public static class ReglasValidacion
    {
        // ---- Empleado ----
        public const string RegexDni = @"^\d{7,8}$";
        public const string MensajeDni = "El DNI debe tener 7 u 8 dígitos, sin puntos ni espacios.";

        public const string RegexLegajo = @"^\d{4,10}$";
        public const string MensajeLegajo = "El legajo debe tener entre 4 y 10 dígitos (ej: 0001).";

        // Letras (con tildes, ñ y ü), espacios, apóstrofo y guion: "María José", "D'Angelo", "Pérez-Gil".
        public const string RegexNombrePersona = @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü' \-]+$";
        public const string MensajeNombrePersona = "Solo se permiten letras, espacios, apóstrofo y guion.";

        // Mismo criterio que ya usaba Proveedor.
        public const string RegexTelefono = @"^\+?[0-9\s\-\(\)]{6,20}$";
        public const string MensajeTelefono = "Ingrese un teléfono válido (6 a 20 caracteres: dígitos, espacios, guiones, paréntesis y + inicial).";

        public const string FechaMinimaIngreso = "1990-01-01";

        // ---- Usuario ----
        public const string RegexUsername = @"^[A-Za-z0-9._]{4,30}$";
        public const string MensajeUsername = "El usuario debe tener entre 4 y 30 caracteres: letras sin tilde, números, punto o guion bajo. Sin espacios.";

        // Mínimo 8, máximo 64, al menos una letra y un número, sin espacios.
        public const string RegexPassword = @"^(?=.*[A-Za-z])(?=.*\d)\S{8,64}$";
        public const string MensajePassword = "La contraseña debe tener entre 8 y 64 caracteres, al menos una letra y un número, y no puede contener espacios.";

        // ---- Proveedor ----
        public const string RegexCuit = @"^\d{2}-\d{8}-\d$";

        // ---- Compra ----
        // Letra/s opcionales + punto de venta (4 o 5 dígitos) + número (1 a 8 dígitos): "FC-0001-1234", "A-00002-00012345", "0001-123".
        public const string RegexComprobante = @"^([A-Za-z]{1,3}-)?\d{4,5}-\d{1,8}$";
        public const string MensajeComprobante = "Formato de comprobante inválido. Ej: FC-0001-1234 o A-00002-00012345.";

        public const string FechaMinimaCompra = "2000-01-01";

        // ---- Receta ----
        public const decimal CantidadRecetaMinima = 0.001m;
        public const decimal CantidadRecetaMaxima = 100000m;
    }
}
