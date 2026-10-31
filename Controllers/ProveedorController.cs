using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    public class ProveedorController : Controller
    {
        private readonly RepositorioProveedor repoProveedor;
        private readonly ILogger<ProveedorController> logger;

        public ProveedorController(RepositorioProveedor repoProveedor, ILogger<ProveedorController> logger)
        {
            this.repoProveedor = repoProveedor;
            this.logger = logger;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            if (pagina < 1) pagina = 1;

            int totalRegistros = repoProveedor.ObtenerCantidad();
            var proveedores = repoProveedor.ObtenerLista(pagina, tamPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
            return View(proveedores);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var resultado = repoProveedor.Buscar(q.Trim()).Select(p => new
            {
                id = p.IdProveedor,
                texto = p.Nombre
            });

            return Json(resultado);
        }

        public IActionResult Alta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Proveedor proveedor)
        {
            NormalizarTextos(proveedor);

            if (!string.IsNullOrEmpty(proveedor.Cuit) && repoProveedor.ExisteCuitActivo(proveedor.Cuit))
                ModelState.AddModelError(nameof(Proveedor.Cuit), "Ya existe un proveedor activo con ese CUIT.");

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            try
            {
                int idGenerado = repoProveedor.Alta(proveedor);
                if (idGenerado > 0)
                {
                    TempData["Mensaje"] = "Proveedor guardado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "No se pudo registrar el proveedor.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar el proveedor {Cuit}", proveedor.Cuit);
                ModelState.AddModelError("", "Ocurrió un error inesperado al registrar el proveedor. Intentá nuevamente.");
            }

            return View(proveedor);
        }

        // El listado ya tenía el botón "Editar" apuntando a esta acción, pero la acción no existía (404).
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var proveedor = repoProveedor.ObtenerPorId(id);
            if (proveedor == null)
            {
                TempData["Error"] = "El proveedor no existe o está dado de baja.";
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Proveedor proveedor)
        {
            // El id de la ruta manda sobre el del formulario.
            proveedor.IdProveedor = id;
            NormalizarTextos(proveedor);

            if (!string.IsNullOrEmpty(proveedor.Cuit) && repoProveedor.ExisteCuitActivo(proveedor.Cuit, id))
                ModelState.AddModelError(nameof(Proveedor.Cuit), "Ya existe otro proveedor activo con ese CUIT.");

            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            try
            {
                if (repoProveedor.Modificar(proveedor))
                {
                    TempData["Mensaje"] = "Proveedor actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                // Modificar devuelve false si no existe/está dado de baja o si no hubo cambios.
                TempData["Mensaje"] = "No se registraron cambios en el proveedor.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al modificar el proveedor {IdProveedor}", id);
                ModelState.AddModelError("", "Ocurrió un error inesperado al modificar el proveedor. Intentá nuevamente.");
            }

            return View(proveedor);
        }

        // POST + antiforgery: el modal de confirmación ya enviaba un form POST con token.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baja(int id)
        {
            try
            {
                bool exito = repoProveedor.Baja(id);

                if (exito)
                {
                    TempData["Mensaje"] = "El proveedor se dio de baja correctamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo realizar la baja del proveedor.";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al dar de baja el proveedor {IdProveedor}", id);
                TempData["Error"] = "Ocurrió un error inesperado al dar de baja el proveedor.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static void NormalizarTextos(Proveedor proveedor)
        {
            proveedor.Nombre = proveedor.Nombre?.Trim();
            proveedor.Cuit = proveedor.Cuit?.Trim();
            proveedor.Telefono = proveedor.Telefono?.Trim();
            proveedor.Direccion = string.IsNullOrWhiteSpace(proveedor.Direccion) ? null : proveedor.Direccion.Trim();
        }
    }
}
