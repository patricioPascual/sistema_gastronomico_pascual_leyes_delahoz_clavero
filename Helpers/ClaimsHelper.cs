using System.Security.Claims;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers
{
    public static class ClaimsHelper
    {
        public static List<Claim> ConstruirClaims(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString()),
                new Claim("IdEmpleado", usuario.IdEmpleado.ToString()),
                new Claim("AvatarPath", usuario.AvatarPath ?? string.Empty)
            };

            if (usuario.Empleado != null)
            {
                claims.Add(new Claim("Nombre", usuario.Empleado.Nombre ?? string.Empty));
                claims.Add(new Claim("Apellido", usuario.Empleado.Apellido ?? string.Empty));
                claims.Add(new Claim("NombreCompleto", $"{usuario.Empleado.Nombre} {usuario.Empleado.Apellido}"));
            }

            return claims;
        }
    }
}
