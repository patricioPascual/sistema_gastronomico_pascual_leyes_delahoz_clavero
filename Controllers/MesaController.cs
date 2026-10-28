using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using MySqlConnector;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Todos)]
    public class MesaController : Controller
    {
        private readonly IConfiguration config;
        private readonly ILogger<MesaController> logger;
        private readonly RepositorioMesa repoMesa;
        private readonly RepositorioPedido repoPedido;

        public MesaController(IConfiguration config, ILogger<MesaController> logger, RepositorioMesa repoMesa, RepositorioPedido repoPedido)
        {
            this.config = config;
            this.logger = logger;
            this.repoMesa = repoMesa;
            this.repoPedido = repoPedido;
        }

        // Sin paginado a propósito: el salón es un conjunto chico y fijo de
        // mesas/puestos (14 en este caso), no un catálogo que crece. Usa la misma
        // fuente de ocupación que Salon() para no tener dos criterios de
        // Libre/Ocupada distintos en la app.
        public IActionResult Index()
        {
            var mesas = repoMesa.ObtenerTodosConOcupacion()
                .OrderBy(m => m.Tipo)
                .ThenBy(m => m.Numero)
                .ToList();

            return View(mesas);
        }

        // Vista principal del salón: mesas + barra, coloreadas según si tienen
        // un pedido abierto, más el offcanvas de pedidos pendientes.
        public IActionResult Salon()
        {
            var todas = repoMesa.ObtenerTodosConOcupacion();

            ViewBag.Mesas = todas.Where(m => m.Tipo == "Mesa").OrderBy(m => m.Numero).ToList();
            ViewBag.Barra = todas.Where(m => m.Tipo == "Barra").OrderBy(m => m.Numero).ToList();
            ViewBag.PedidosPendientes = repoPedido.ObtenerAbiertos();

            return View();
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
            int tamanoMax = 12;
            if(mesa.Capacidad > tamanoMax)
            {
                TempData["Error"] = "La capacidad no debe ser mayor a 12";
                return View("Alta", mesa);
            }
            if (!ModelState.IsValid)
                return View("Alta", mesa);

            try
            {
                repoMesa.Alta(mesa);
                TempData["Mensaje"] = "Mesa registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                ModelState.AddModelError("Numero", "Ya existe una mesa con el número " + mesa.Numero + ".");
                return View("Alta", mesa);
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
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1451)
            {
                TempData["Error"] = "No se puede eliminar la mesa: tiene pedidos asociados.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Metodo agregado para implementar VUE en Salon
        [HttpGet]
        public IActionResult ObtenerEstadoSalonAjax()
        {
            var todas = repoMesa.ObtenerTodosConOcupacion();

            var mesas = todas.Where(m => m.Tipo == "Mesa").OrderBy(m => m.Numero).ToList();
            var barra = todas.Where(m => m.Tipo == "Barra").OrderBy(m => m.Numero).ToList();

            var pendientes = repoPedido.ObtenerPedidosConDetallesEnMarcha().Select(p => new
            {
                idPedido = p.IdPedido,
                mesaNumero = p.Mesa != null ? p.Mesa.Numero : 0,
                empleadoNombre = p.Empleado != null ? $"{p.Empleado.Apellido}, {p.Empleado.Nombre}" : "Sin asignar",
                fechaHora = p.FechaHora.ToString("o"),
                cerrado = p.estado == Pedido.Estado.Pagado, // la mesa ya se cobró, pero este plato sigue en marcha
                detalles = p.Detalles.Select(d => new
                {
                    idDetallePedido = d.IdDetallePedido,
                    cantidad = d.Cantidad,
                    nombrePlato = d.Plato != null ? d.Plato.Nombre : "Plato",
                    fechaHora = d.FechaHora.ToString("o")
                }).ToList()
            }).ToList();

            return Json(new { mesas, barra, pendientes });
        }
    }
}