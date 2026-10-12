namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers
{
    /// <summary>
    /// Genera las iniciales a mostrar cuando un usuario no tiene foto de perfil cargada.
    /// </summary>
    public static class AvatarHelper
    {
        public static string ObtenerIniciales(string? nombre, string? apellido)
        {
            var inicialNombre = !string.IsNullOrWhiteSpace(nombre) ? nombre.Trim()[0].ToString() : string.Empty;
            var inicialApellido = !string.IsNullOrWhiteSpace(apellido) ? apellido.Trim()[0].ToString() : string.Empty;

            var iniciales = (inicialNombre + inicialApellido).ToUpperInvariant();

            return string.IsNullOrEmpty(iniciales) ? "?" : iniciales;
        }
    }
}
