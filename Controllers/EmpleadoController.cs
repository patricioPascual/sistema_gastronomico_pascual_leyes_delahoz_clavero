using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using MySqlConnector;  // en lugar de MySql.Data.MySqlClient
using Microsoft.EntityFrameworkCore;   // para DbUpdateException
using Microsoft.AspNetCore.Authorization;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    public class EmpleadoController : Controller
    {
        private readonly RepositorioEmpleado repoEmpleado;

        public EmpleadoController(RepositorioEmpleado repoEmpleado)
        {
            this.repoEmpleado = repoEmpleado;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            var empleados = repoEmpleado.ObtenerLista(pagNro: pagina, tamPagina: tamPagina);
            int totalRegistros = repoEmpleado.ObtenerCantidad();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
            return View(empleados);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var empleados = repoEmpleado.Buscar(q);

            var resultado = empleados.Select(e => new
            {
                id = e.IdEmpleado,
                text = $"{e.Apellido}, {e.Nombre} (legajo {e.Legajo})"
            });

            return Json(resultado);
        }

        public IActionResult Alta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Empleado empleado)
        {
            NormalizarTextos(empleado);
            empleado.Activo = true;

            ValidarDuplicados(empleado, excluirId: 0);

            if (!ModelState.IsValid)
                return View("Alta", empleado);

            try
            {
                repoEmpleado.Alta(empleado);
                TempData["Mensaje"] = "Empleado registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                ModelState.AddModelError("", "Ya existe un empleado con ese legajo o DNI.");
                return View("Alta", empleado);
            }
        }

        public IActionResult Editar(int id)
        {
            var empleado = repoEmpleado.ObtenerPorId(id);
            if (empleado == null)
                return NotFound();

            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Empleado empleado)
        {
            NormalizarTextos(empleado);

            ValidarDuplicados(empleado, excluirId: empleado.IdEmpleado);

            if (!ModelState.IsValid)
                return View("Editar", empleado);

            try
            {
                repoEmpleado.Modificar(empleado);
                TempData["Mensaje"] = "Empleado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            // EF Core envuelve la MySqlException en DbUpdateException: atrapar MySqlException directo no funcionaba.
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                ModelState.AddModelError("", "Ya existe un empleado con ese legajo o DNI.");
                return View("Editar", empleado);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baja(int id)
        {
            repoEmpleado.Baja(id);
            TempData["Mensaje"] = "Empleado dado de baja correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private static void NormalizarTextos(Empleado empleado)
        {
            empleado.Nombre = empleado.Nombre?.Trim();
            empleado.Apellido = empleado.Apellido?.Trim();
            empleado.Telefono = string.IsNullOrWhiteSpace(empleado.Telefono) ? null : empleado.Telefono.Trim();
        }

        private void ValidarDuplicados(Empleado empleado, int excluirId)
        {
            if (!string.IsNullOrEmpty(empleado.Legajo) && repoEmpleado.ExisteLegajo(empleado.Legajo, excluirId))
                ModelState.AddModelError(nameof(Empleado.Legajo), "Ya existe un empleado con ese legajo.");

            if (!string.IsNullOrEmpty(empleado.Dni) && repoEmpleado.ExisteDni(empleado.Dni, excluirId))
                ModelState.AddModelError(nameof(Empleado.Dni), "Ya existe un empleado con ese DNI.");
        }
    }
}
