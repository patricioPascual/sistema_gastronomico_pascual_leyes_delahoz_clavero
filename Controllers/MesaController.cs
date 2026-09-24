using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class MesaController : Controller
    {
        private readonly IConfiguration config;
        private readonly ILogger<MesaController> logger;
        private readonly RepositorioMesa repoMesa;

        public MesaController(IConfiguration config, ILogger<MesaController> logger, RepositorioMesa repoMesa)
        {
            this.config = config;
            this.logger = logger;
            this.repoMesa = repoMesa;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            var mesas = repoMesa.ObtenerLista(pagNro: pagina, tamPagina: tamPagina);
            int totalRegistros = repoMesa.ObtenerCantidad();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
            return View(mesas);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var mesas = repoMesa.Buscar(q);

            var resultado = mesas.Select(m => new
            {
                id = m.IdMesa,
                text = $"Mesa {m.Numero} ({m.Capacidad} personas)"
            });

            return Json(resultado);
        }

        public IActionResult Alta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Mesa mesa)
        {
            if (!ModelState.IsValid)
                return View("Alta", mesa);

            try
            {
                repoMesa.Alta(mesa);
                TempData["Mensaje"] = "Mesa registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError("Numero", "Ya existe una mesa con el número " + mesa.Numero + ".");
                return View("Alta", mesa);
            }
        }

        public IActionResult Editar(int id)
        {
            var mesa = repoMesa.ObtenerPorId(id);
            if (mesa == null)
                return NotFound();

            return View(mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Mesa mesa)
        {
            if (!ModelState.IsValid)
                return View("Editar", mesa);

            try
            {
                repoMesa.Modificar(mesa);
                TempData["Mensaje"] = "Mesa actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError("Numero", "Ya existe una mesa con el número " + mesa.Numero + ".");
                return View("Editar", mesa);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baja(int id)
        {
            try
            {
                repoMesa.Baja(id);
                TempData["Mensaje"] = "Mesa eliminada correctamente.";
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                TempData["Error"] = "No se puede eliminar la mesa: tiene pedidos asociados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}