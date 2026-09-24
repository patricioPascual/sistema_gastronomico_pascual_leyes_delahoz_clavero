using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

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

        public IActionResult Index(int pagina=1)
        {
            int tamPagina = 10; 
            var Productos = repoProducto.ObtenerLista(pagNro: pagina, tamPagina: tamPagina);
            int totalRegistros= repoProducto.ObtenerCantidad();

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


    }
}