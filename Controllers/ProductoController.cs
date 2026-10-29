using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Administrador)]
    public class ProductoController : Controller
    {
        private readonly RepositorioProducto _repositorioProducto;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(RepositorioProducto repositorioProducto, ILogger<ProductoController> logger)
        {
            _repositorioProducto = repositorioProducto;
            _logger = logger;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            int totalRegistros = _repositorioProducto.ObtenerCantidad();

            var productos = _repositorioProducto.ObtenerLista(pagina, tamPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
            return View(productos);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var productos = _repositorioProducto.Buscar(q);

            var resultado = productos.Select(p => new
            {
                id = p.IdProducto,
                text = $"{p.Nombre} ({p.Cantidad_stock} {p.Unidad_medida})"
            });

            return Json(resultado);
        }

        public IActionResult Alta()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Producto producto)
        {
            producto.Nombre = producto.Nombre?.Trim();
            producto.Estado = true;

            ValidarNombreDuplicado(producto);

            if (!ModelState.IsValid)
                return View("Alta", producto);

            try
            {
                _repositorioProducto.Alta(producto);
                TempData["Mensaje"] = "Insumo registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            // Red de seguridad: dos altas simultáneas con el mismo nombre que pasaron el chequeo previo.
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                ModelState.AddModelError(nameof(Producto.Nombre), $"El insumo '{producto.Nombre}' ya existe.");
                return View("Alta", producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el producto {Nombre}", producto.Nombre);
                ModelState.AddModelError("", "Ocurrió un error inesperado al registrar el insumo. Intentá nuevamente.");
                return View("Alta", producto);
            }
        }

        [HttpGet]
        public IActionResult BuscarTexto(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var productos = _repositorioProducto.Buscar(q);

            var resultado = productos.Select(p => new
            {
                id = p.IdProducto,
                texto = p.Nombre,
                unidad = p.Unidad_medida,
                precioCosto = p.Precio_costo
            });

            return Json(resultado);
        }

        [HttpGet]
        public IActionResult Modificar(int id)
        {
            var producto = _repositorioProducto.ObtenerPorId(id);

            if (producto == null)
            {
                TempData["Error"] = "El producto que intenta modificar no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Producto producto)
        {
            producto.Nombre = producto.Nombre?.Trim();

            ValidarNombreDuplicado(producto);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Verifique los datos ingresados. Hay campos inválidos.";
                return View(producto);
            }

            try
            {
                int filas = _repositorioProducto.Modificar(producto);

                if (filas > 0)
                {
                    TempData["Exito"] = "El producto se modificó correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "No se pudo modificar el producto (es posible que no haya habido cambios).";
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                ModelState.AddModelError(nameof(Producto.Nombre), $"El insumo '{producto.Nombre}' ya existe.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar el producto {IdProducto}", producto.IdProducto);
                TempData["Error"] = "Ocurrió un error inesperado al modificar el producto. Intentá nuevamente.";
            }

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                int filas = _repositorioProducto.Baja(id);
                if (filas > 0)
                {
                    TempData["Exito"] = "El producto se dio de baja correctamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo encontrar el producto.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al dar de baja el producto {IdProducto}", id);
                TempData["Error"] = "Ocurrió un error inesperado al dar de baja el producto.";
            }

            return RedirectToAction(nameof(Index));
        }

        // El índice uq_producto_nombre abarca activos e inactivos, así que se busca en todos
        // y se avisa cuando el duplicado está dado de baja (se puede reactivar desde Modificar).
        private void ValidarNombreDuplicado(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return;

            var existente = _repositorioProducto.ObtenerPorNombre(producto.Nombre, producto.IdProducto);
            if (existente == null)
                return;

            string mensaje = existente.Estado
                ? $"Ya existe un insumo llamado '{existente.Nombre}'."
                : $"Ya existe un insumo llamado '{existente.Nombre}', pero está dado de baja. Reactivalo desde Modificar en lugar de crear uno nuevo.";

            ModelState.AddModelError(nameof(Producto.Nombre), mensaje);
        }
    }
}
