using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly RepositorioUsuario repoUsuario;
        private readonly RepositorioEmpleado repoEmpleado;
        private readonly RepositorioRol repoRol;
        private readonly GastronomiaContext context;
        private readonly IPasswordHasher<Usuario> passwordHasher;
        private readonly IWebHostEnvironment entorno;

        private const long TamanioMaximoAvatarBytes = 2 * 1024 * 1024; // 2 MB
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        public UsuarioController(
            RepositorioUsuario repoUsuario,
            RepositorioEmpleado repoEmpleado,
            RepositorioRol repoRol,
            GastronomiaContext context,
            IPasswordHasher<Usuario> passwordHasher,
            IWebHostEnvironment entorno)
        {
            this.repoUsuario = repoUsuario;
            this.repoEmpleado = repoEmpleado;
            this.repoRol = repoRol;
            this.context = context;
            this.passwordHasher = passwordHasher;
            this.entorno = entorno;
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        public IActionResult Index()
        {
            var usuarios = repoUsuario.ObtenerTodos();
            return View(usuarios);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        public IActionResult Registrar()
        {
            var vm = new RegistrarUsuarioViewModel
            {
                RolesDisponibles = repoRol.ObtenerTodos().ToList()
            };
            return View(vm);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(RegistrarUsuarioViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.RolesDisponibles = repoRol.ObtenerTodos().ToList();
                return View(vm);
            }

            using var transaccion = context.Database.BeginTransaction();

            try
            {
                var empleado = new Empleado
                {
                    Legajo = vm.Legajo,
                    Nombre = vm.Nombre,
                    Apellido = vm.Apellido,
                    Dni = vm.Dni,
                    Telefono = vm.Telefono,
                    Fecha_ingreso = vm.FechaIngreso,
                    Activo = true
                };

                var idEmpleado = repoEmpleado.Alta(empleado);

                var usuario = new Usuario
                {
                    IdEmpleado = idEmpleado,
                    IdRol = vm.IdRol,
                    Username = vm.Username,
                    Estado = true
                };

                usuario.PasswordHash = passwordHasher.HashPassword(usuario, vm.Password ?? string.Empty);

                repoUsuario.Alta(usuario);

                transaccion.Commit();

                TempData["Mensaje"] = "Usuario registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                transaccion.Rollback();
                ModelState.AddModelError("", "Ya existe un empleado o usuario con esos datos (legajo, DNI o nombre de usuario duplicado).");
                vm.RolesDisponibles = repoRol.ObtenerTodos().ToList();
                return View(vm);
            }
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var usuario = repoUsuario.ObtenerPorId(id);
            if (usuario == null || usuario.Empleado == null)
            {
                return NotFound();
            }

            var vm = new EditarUsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                IdEmpleado = usuario.IdEmpleado,
                Legajo = usuario.Empleado.Legajo,
                Nombre = usuario.Empleado.Nombre,
                Apellido = usuario.Empleado.Apellido,
                Dni = usuario.Empleado.Dni,
                Telefono = usuario.Empleado.Telefono,
                IdRol = usuario.IdRol,
                Username = usuario.Username,
                RolesDisponibles = repoRol.ObtenerTodos().ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarUsuarioViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.NuevaPassword) && vm.NuevaPassword.Length < 6)
            {
                ModelState.AddModelError(nameof(vm.NuevaPassword), "La contraseña debe tener al menos 6 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                vm.RolesDisponibles = repoRol.ObtenerTodos().ToList();
                return View(vm);
            }

            var usuario = repoUsuario.ObtenerPorId(vm.IdUsuario);
            if (usuario == null || usuario.Empleado == null)
            {
                return NotFound();
            }

            using var transaccion = context.Database.BeginTransaction();

            try
            {
                usuario.Empleado.Legajo = vm.Legajo;
                usuario.Empleado.Nombre = vm.Nombre;
                usuario.Empleado.Apellido = vm.Apellido;
                usuario.Empleado.Dni = vm.Dni;
                usuario.Empleado.Telefono = vm.Telefono;
                repoEmpleado.Modificar(usuario.Empleado);

                usuario.Username = vm.Username;
                usuario.IdRol = vm.IdRol;

                if (!string.IsNullOrEmpty(vm.NuevaPassword))
                {
                    usuario.PasswordHash = passwordHasher.HashPassword(usuario, vm.NuevaPassword);
                }

                repoUsuario.Modificar(usuario);

                transaccion.Commit();

                // Si el admin se edita a sí mismo (cambia su rol, username o contraseña), refrescamos
                // su propia cookie para que los claims no queden desactualizados en esta misma sesión.
                var idUsuarioLogueado = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (idUsuarioLogueado == usuario.IdUsuario.ToString())
                {
                    var claims = ClaimsHelper.ConstruirClaims(usuario);
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                }

                TempData["Mensaje"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                transaccion.Rollback();
                ModelState.AddModelError("", "Ya existe un empleado o usuario con esos datos (legajo, DNI o nombre de usuario duplicado).");
                vm.RolesDisponibles = repoRol.ObtenerTodos().ToList();
                return View(vm);
            }
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baja(int id)
        {
            var usuario = repoUsuario.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            using (var transaccion = context.Database.BeginTransaction())
            {
                repoUsuario.Baja(id);
                repoEmpleado.Baja(usuario.IdEmpleado);
                transaccion.Commit();
            }

            TempData["Mensaje"] = "Usuario dado de baja correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Perfil()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(IFormFile? avatar)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
            {
                return NotFound();
            }

            if (avatar == null || avatar.Length == 0)
            {
                ModelState.AddModelError("", "Elegí una imagen antes de guardar.");
                return View(usuario);
            }

            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension) || !avatar.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("", "Formato no permitido. Usá una imagen JPG, PNG o WEBP.");
                return View(usuario);
            }

            if (avatar.Length > TamanioMaximoAvatarBytes)
            {
                ModelState.AddModelError("", "La imagen no puede superar los 2 MB.");
                return View(usuario);
            }

            var carpetaAvatares = Path.Combine(entorno.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(carpetaAvatares);

            if (!string.IsNullOrEmpty(usuario.AvatarPath))
            {
                var rutaFisicaAnterior = Path.Combine(entorno.WebRootPath, usuario.AvatarPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(rutaFisicaAnterior))
                {
                    System.IO.File.Delete(rutaFisicaAnterior);
                }
            }

            var nombreArchivo = $"{usuario.IdUsuario}_{Guid.NewGuid():N}{extension}";
            var rutaFisicaNueva = Path.Combine(carpetaAvatares, nombreArchivo);

            using (var stream = new FileStream(rutaFisicaNueva, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            usuario.AvatarPath = $"/uploads/avatars/{nombreArchivo}";
            repoUsuario.Modificar(usuario);

            var claims = ClaimsHelper.ConstruirClaims(usuario);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Mensaje"] = "Foto de perfil actualizada correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        private Usuario? ObtenerUsuarioActual()
        {
            var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioClaim, out var idUsuario))
            {
                return null;
            }

            return repoUsuario.ObtenerPorId(idUsuario);
        }
    }
}