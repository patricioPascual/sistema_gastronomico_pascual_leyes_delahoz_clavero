using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class CompraController : Controller
    {
        private readonly RepositorioCompra _repositorioCompra;
        private readonly RepositorioProducto _repositorioProducto;

        public CompraController(RepositorioCompra repositorioCompra , RepositorioProducto repositorioProducto)
        {
            _repositorioCompra = repositorioCompra;
            _repositorioProducto =  repositorioProducto;
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
            // Se puede cargar la lista de productos disponibles para que el usuario los seleccione en el combo del formulario
            ViewBag.Productos = _repositorioProducto.ObtenerLista(1, 100); 
            return View();
        }

        // POST: Compra/Alta
      [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Alta(Compra compra)
{
    try
    {    
        // Empleado De Prueba 
        compra.IdEmpleado = 1;


        if (compra.Detalles == null || compra.Detalles.Count == 0)
        {
            ModelState.AddModelError("", "Debe agregar al menos un insumo o producto a la compra.");
        }

        if (ModelState.IsValid)
        {
            int idCreado = _repositorioCompra.Alta(compra);

            if (idCreado > 0)
            {
                TempData["Exito"] = "La compra fue registrada exitosamente y el stock de los productos ha sido actualizado.";
                return RedirectToAction("Index","Producto");
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
    ViewBag.Productos = _repositorioProducto.ObtenerLista(1, 100);
    return View(compra);
}
    }
}
    
