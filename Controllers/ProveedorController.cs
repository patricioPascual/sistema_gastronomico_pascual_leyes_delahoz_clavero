using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    public class ProveedorController : Controller
    {
        private readonly RepositorioProveedor repoProveedor;

        public ProveedorController(RepositorioProveedor repoProveedor)
        {
            this.repoProveedor = repoProveedor;
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


        public IActionResult Alta()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Proveedor proveedor)
        {
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
                ModelState.AddModelError("", "Ocurrió un error inesperado: " + ex.Message);
            }

            return View(proveedor);
        }

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
                TempData["Error"] = "Error al intentar eliminar el proveedor: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }

}