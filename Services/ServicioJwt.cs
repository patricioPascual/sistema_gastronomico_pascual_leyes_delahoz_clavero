using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using Microsoft.IdentityModel.Tokens;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Services
{
    public class ServicioJwt
    {
        private readonly IConfiguration config;

        public ServicioJwt(IConfiguration configuration)
        {
            config = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            // 1. Definición de Claims usando las propiedades reales de TU modelo Usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username ?? string.Empty),
                // Guardamos el IdRol. Si tuvieras el nombre del rol en string, iría acá en lugar del ToString()
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString()),
                new Claim("IdEmpleado", usuario.IdEmpleado.ToString())
            };

            // 2. Obtención de clave con Encoding ASCII buscando en "TokenAuthentication" (según el PDF)
            // Se usa el operador ?? para evitar nulos si la clave no se encuentra en el appsettings
            var keyStr = config["TokenAuthentication:SecretKey"] ?? string.Empty;
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(keyStr));

            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Generación del Token
            var token = new JwtSecurityToken(
                issuer: config["TokenAuthentication:Issuer"],
                audience: config["TokenAuthentication:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credenciales
            );

            // 4. Retorno del Token comprimido
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}