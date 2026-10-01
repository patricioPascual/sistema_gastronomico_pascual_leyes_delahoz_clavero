using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
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
            if (!string.IsNullOrWhiteSpace(producto.Nombre))
            {
                producto.Nombre = producto.Nombre.Trim();
            }

            producto.Estado = true;

            if (!ModelState.IsValid)
                return View(producto);

            try
            {
                _repositorioProducto.Alta(producto);
                TempData["Mensaje"] = "Insumo registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Nombre", "El insumo '" + producto.Nombre + "' ya existe en la base de datos.");
                return View(producto);
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
            try
            {
                if (ModelState.IsValid)
                {
                    int filas = _repositorioProducto.Modificar(producto);

                    if (filas > 0)
                    {
                        TempData["Exito"] = "El producto se modificó correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo modificar el producto (es posible que no haya habido cambios).";
                    }
                }
                else
                {
                    TempData["Error"] = "Verifique los datos ingresados. Hay campos inválidos.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error en base de datos: " + ex.Message;
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
                TempData["Error"] = "Error al intentar eliminar el producto: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}