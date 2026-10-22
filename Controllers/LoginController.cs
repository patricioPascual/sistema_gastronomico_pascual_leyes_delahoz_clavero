using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly RepositorioUsuario repoUsuario;
        private readonly IPasswordHasher<Usuario> passwordHasher;

        public LoginController(RepositorioUsuario repoUsuario, IPasswordHasher<Usuario> passwordHasher)
        {
            this.repoUsuario = repoUsuario;
            this.passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("2"))
                {
                    return RedirectToAction("Index", "SPA");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var usuario = repoUsuario.ObtenerPorUsername(vm.Username ?? string.Empty);

            if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(vm);
            }

            var resultadoVerificacion = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, vm.Password ?? string.Empty);

            if (resultadoVerificacion == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(vm);
            }

            if (resultadoVerificacion == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = passwordHasher.HashPassword(usuario, vm.Password ?? string.Empty);
                repoUsuario.Modificar(usuario);
            }

            var claims = ClaimsHelper.ConstruirClaims(usuario);

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (usuario.IdRol == 2)
            {
                return RedirectToAction("Index", "SPA");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
