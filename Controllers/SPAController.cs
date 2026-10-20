using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Services;

namespace sistema_gastronomico.Controllers
{
    [Authorize(Roles = "2")]
    public class SPAController : Controller
    {
        private readonly RepositorioUsuario _repoUsuario;
        private readonly ServicioJwt _servicioJwt;

        public SPAController(RepositorioUsuario repoUsuario, ServicioJwt servicioJwt)
        {
            _repoUsuario = repoUsuario;
            _servicioJwt = servicioJwt;
        }

        public IActionResult Index()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var usuario = _repoUsuario.ObtenerPorUsername(username);

            if (usuario == null || usuario.IdRol != 2)
                return Forbid();

            ViewBag.TokenApi = _servicioJwt.GenerarToken(usuario);
            return View();
        }
    }
}