using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class ProductoController : Controller
    {
        private readonly IConfiguration config;
        private readonly ILogger<ProductoController> logger;
        private readonly RepositorioProducto repoProducto;

        public ProductoController(IConfiguration config, ILogger<ProductoController> logger, RepositorioProducto repoProducto)
        {
            this.config = config;
            this.logger = logger;
            this.repoProducto = repoProducto;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            var Productos = repoProducto.ObtenerLista(pagNro: pagina, tamPagina: tamPagina);
            int totalRegistros = repoProducto.ObtenerCantidad();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
            return View(Productos);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var productos = repoProducto.Buscar(q);

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
            // 1. Limpieza de espacios y estandarizacion de texto
            if (!string.IsNullOrWhiteSpace(producto.Nombre))
            {
                producto.Nombre = producto.Nombre.Trim();
            }

            if (!ModelState.IsValid)
                return View(producto);

            try
            {
                repoProducto.Alta(producto);
                TempData["Mensaje"] = "Insumo registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1062)
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

            var productos = repoProducto.BuscarPorTexto(q)
                .Select(p => new
                {
                    id = p.IdProducto,
                    texto = p.Nombre,
                    unidad = p.Unidad_medida
                });

            return Json(productos);
        }

    [HttpGet]
public IActionResult Modificar (int id)
{
    var producto = repoProducto.ObtenerPorId(id);

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
        
            bool exito = repoProducto.Modificar(producto) >0 ;

            if (exito)
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
        bool exito = repoProducto.Baja(id) > 0;

        if (exito)
        {
            TempData["Exito"] = "El producto se dio de baja correctamente.";
        }
        else
        {
            TempData["Error"] = "No se pudo realizar la baja del producto.";
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