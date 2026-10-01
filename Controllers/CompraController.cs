using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class CompraController : Controller
    {
        private readonly RepositorioCompra _repositorioCompra;
        private readonly RepositorioProducto _repositorioProducto;
        private readonly RepositorioProveedor _repositorioProveedor;

        public CompraController(
            RepositorioCompra repositorioCompra, 
            RepositorioProducto repositorioProducto, 
            RepositorioProveedor repositorioProveedor)
        {
            _repositorioCompra = repositorioCompra;
            _repositorioProducto = repositorioProducto;
            _repositorioProveedor = repositorioProveedor;
        }

        [HttpGet]
        public IActionResult Index(int pagNro = 1, int tamPagina = 10)
        {
            try
            {
                if (pagNro < 1) pagNro = 1;
                if (tamPagina < 1) tamPagina = 10;

                List<Compra> listaCompras = _repositorioCompra.ObtenerLista(pagNro, tamPagina);

                ViewBag.PagNro = pagNro;
                ViewBag.TamPagina = tamPagina;

                return View(listaCompras);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al cargar el listado de compras: " + ex.Message;
                return View(new List<Compra>());
            }
        }

        [HttpGet]
        public IActionResult Alta()
        {
            ViewBag.Productos = _repositorioProducto.Buscar(""); 
            ViewBag.Proveedores = _repositorioProveedor.ObtenerTodos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Compra compra)
        {
            try
            {    
                // Empleado de prueba
                compra.IdEmpleado = 1;

                // AJUSTE CLAVE ORM: Ignoramos la validación de objetos de navegación 
                // que no vienen en el formulario POST
              //  ModelState.Remove(nameof(compra.Proveedor));
                //ModelState.Remove(nameof(compra.Empleado));

                if (compra.Detalles == null || compra.Detalles.Count == 0)
                {
                    ModelState.AddModelError("", "Debe agregar al menos un insumo o producto a la compra.");
                }

                if (ModelState.IsValid)
                {   
                    compra.TotalCompra = compra.Detalles?.Sum(d => d.CantidadIngresada * d.PrecioCostoUnitario) ?? 0;
                    int idCreado = _repositorioCompra.Alta(compra);

                    if (idCreado > 0)
                    {
                        TempData["Exito"] = "La compra fue registrada exitosamente y el stock de los productos ha sido actualizado.";
                        return RedirectToAction("Index", "Producto");
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo registrar la compra en la base de datos.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al procesar la compra: " + ex.Message;
            }

            ViewBag.Productos = _repositorioProducto.Buscar("");
            ViewBag.Proveedores = _repositorioProveedor.ObtenerTodos();
            return View(compra);
        }

        [HttpGet]
        public IActionResult Modificar(int id)
        {
            var compra = _repositorioCompra.ObtenerPorId(id);

            if (compra == null)
            {
                TempData["Error"] = "La compra solicitada no existe o fue anulada.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Productos = _repositorioProducto.Buscar("");
            ViewBag.Proveedores = _repositorioProveedor.ObtenerTodos();

            return View(compra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Compra compra)
        {
            try
            {
                ModelState.Remove(nameof(compra.Proveedor));
                ModelState.Remove(nameof(compra.Empleado));

                if (compra.Detalles == null || compra.Detalles.Count == 0)
                {
                    ModelState.AddModelError("", "Debe agregar al menos un insumo o producto a la compra.");
                }

                if (ModelState.IsValid)
                {
                    bool exito = _repositorioCompra.Modificar(compra);

                    if (exito)
                    {
                        TempData["Exito"] = $"La compra #{compra.IdCompra} fue modificada y el stock fue reajustado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo actualizar la compra en la base de datos.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al modificar la compra: " + ex.Message;
            }

            ViewBag.Productos = _repositorioProducto.Buscar("");
            ViewBag.Proveedores = _repositorioProveedor.ObtenerTodos();
            return View(compra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var compra = _repositorioCompra.ObtenerPorId(id);
                if (compra == null)
                {
                    TempData["Error"] = "La compra que intenta eliminar no existe.";
                    return RedirectToAction(nameof(Index));
                }
                
                bool exito = _repositorioCompra.Baja(id);

                if (exito)
                {
                    TempData["Exito"] = $"La compra #{id} fue anulada exitosamente y se descontó el stock de los productos.";
                }
                else
                {
                    TempData["Error"] = $"No se pudo anular la compra #{id}.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al intentar anular la compra: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}