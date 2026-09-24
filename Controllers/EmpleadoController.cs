using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class EmpleadoController : Controller
    {
        private readonly IConfiguration config;
        private readonly ILogger<EmpleadoController> logger;
        private readonly RepositorioEmpleado repoEmpleado;

        public EmpleadoController(IConfiguration config, ILogger<EmpleadoController> logger, RepositorioEmpleado repoEmpleado)
        {
            this.config = config;
            this.logger = logger;
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
            if (!string.IsNullOrWhiteSpace(empleado.Nombre))
                empleado.Nombre = empleado.Nombre.Trim();

            if (!string.IsNullOrWhiteSpace(empleado.Apellido))
                empleado.Apellido = empleado.Apellido.Trim();

            empleado.Activo = true;

            if (!ModelState.IsValid)
                return View("Alta", empleado);

            try
            {
                repoEmpleado.Alta(empleado);
                TempData["Mensaje"] = "Empleado registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
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
            if (!ModelState.IsValid)
                return View("Editar", empleado);

            try
            {
                repoEmpleado.Modificar(empleado);
                TempData["Mensaje"] = "Empleado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
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
    }
}