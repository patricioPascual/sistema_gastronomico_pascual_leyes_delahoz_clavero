using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using System.Security.Claims;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    public class CompraController : Controller
    {
        private readonly RepositorioCompra _repositorioCompra;
        private readonly RepositorioProducto _repositorioProducto;
        private readonly RepositorioProveedor _repositorioProveedor;
        private readonly ILogger<CompraController> _logger;

        public CompraController(
            RepositorioCompra repositorioCompra,
            RepositorioProducto repositorioProducto,
            RepositorioProveedor repositorioProveedor,
            ILogger<CompraController> logger)
        {
            _repositorioCompra = repositorioCompra;
            _repositorioProducto = repositorioProducto;
            _repositorioProveedor = repositorioProveedor;
            _logger = logger;
        }

        // Validaciones de servidor que no cubre el HTML: detalles vacíos, productos repetidos,
        // proveedor/productos inexistentes o dados de baja, y comprobante repetido para el proveedor.
        // (Formato del comprobante y rango de la fecha los valida el modelo Compra con atributos.)
        private void ValidarCompra(Compra compra)
        {
            compra.NumeroComprobante = string.IsNullOrWhiteSpace(compra.NumeroComprobante)
                ? null
                : compra.NumeroComprobante.Trim().ToUpperInvariant();

            // Replica el índice uk_proveedor_comprobante para dar un mensaje claro en vez del error de MySQL.
            if (compra.NumeroComprobante != null && compra.IdProveedor > 0
                && _repositorioCompra.ExisteComprobante(compra.IdProveedor, compra.NumeroComprobante, compra.IdCompra))
            {
                ModelState.AddModelError(nameof(Compra.NumeroComprobante),
                    $"Ya hay una compra registrada con el comprobante {compra.NumeroComprobante} para este proveedor.");
            }

            if (compra.Detalles == null || compra.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un insumo o producto a la compra.");
                return;
            }

            var proveedor = _repositorioProveedor.ObtenerPorId(compra.IdProveedor);
            if (proveedor == null || !proveedor.Estado)
                ModelState.AddModelError("", "El proveedor seleccionado no existe o está dado de baja.");

            if (compra.Detalles.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1))
                ModelState.AddModelError("", "No se puede repetir un mismo producto en la compra. Unifique las cantidades en una sola línea.");

            foreach (var detalle in compra.Detalles)
            {
                var producto = _repositorioProducto.ObtenerPorId(detalle.IdProducto);
                if (producto == null || !producto.Estado)
                {
                    ModelState.AddModelError("", "Uno de los productos seleccionados no existe o está dado de baja.");
                    break;
                }
            }
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
                _logger.LogError(ex, "Error al cargar el listado de compras");
                TempData["Error"] = "Ocurrió un error al cargar el listado de compras.";
                return View(new List<Compra>());
            }
        }

        [HttpGet]
        public IActionResult Alta()
        {
            ViewBag.Productos = _repositorioProducto.Buscar(""); 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Compra compra)
        {
            try
            {    
                // El empleado que registra la compra es siempre el usuario logueado (claim de la cookie),
                // nunca el valor que llegue en el formulario.
                if (!int.TryParse(User.FindFirst("IdEmpleado")?.Value, out var idEmpleado) || idEmpleado <= 0)
                {
                    TempData["Error"] = "No se pudo identificar al empleado logueado. Volvé a iniciar sesión.";
                    return RedirectToAction("Index", "Login");
                }
                compra.IdEmpleado = idEmpleado;
                ModelState.Remove(nameof(Compra.IdEmpleado));

                ValidarCompra(compra);

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
                _logger.LogError(ex, "Error al registrar la compra del proveedor {IdProveedor}", compra.IdProveedor);
                TempData["Error"] = "Ocurrió un error inesperado al procesar la compra. Intentá nuevamente.";
            }

            ViewBag.Productos = _repositorioProducto.Buscar("");
            ViewBag.ProveedorSeleccionado = _repositorioProveedor.ObtenerPorId(compra.IdProveedor);
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
            ViewBag.ProveedorSeleccionado = _repositorioProveedor.ObtenerPorId(compra.IdProveedor);

            return View(compra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Compra compra)
        {
            try
            {

                ValidarCompra(compra);

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
                _logger.LogError(ex, "Error al modificar la compra {IdCompra}", compra.IdCompra);
                TempData["Error"] = "Ocurrió un error inesperado al modificar la compra. Intentá nuevamente.";
            }

            ViewBag.Productos = _repositorioProducto.Buscar("");
            ViewBag.ProveedorSeleccionado = _repositorioProveedor.ObtenerPorId(compra.IdProveedor);
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
                _logger.LogError(ex, "Error al anular la compra {IdCompra}", id);
                TempData["Error"] = "Ocurrió un error inesperado al anular la compra.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}