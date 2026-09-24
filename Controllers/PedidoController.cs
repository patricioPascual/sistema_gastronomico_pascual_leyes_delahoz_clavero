using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class PedidoController : Controller
    {
        private readonly RepositorioPedido repoPedido;
        private readonly RepositorioDetallePedido repoDetalle;
        private readonly RepositorioMesa repoMesa;
        private readonly RepositorioEmpleado repoEmpleado;
        private readonly IRepositorioPlato repoPlato;

        public PedidoController(RepositorioPedido repoPedido, RepositorioDetallePedido repoDetalle,
            RepositorioMesa repoMesa, RepositorioEmpleado repoEmpleado, IRepositorioPlato repoPlato)
        {
            this.repoPedido = repoPedido;
            this.repoDetalle = repoDetalle;
            this.repoMesa = repoMesa;
            this.repoEmpleado = repoEmpleado;
            this.repoPlato = repoPlato;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            if (pagina < 1) pagina = 1;

            try
            {
                var pedidos = repoPedido.ObtenerLista(pagina, tamPagina);
                int totalRegistros = repoPedido.ObtenerCantidad();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);
                return View(pedidos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al cargar el listado de pedidos: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        public IActionResult Detalle(int id)
        {
            var pedido = repoPedido.ObtenerPorId(id);
            if (pedido == null)
                return NotFound();

            // Platos para el formulario de agregar un renglón al pedido
            ViewBag.Platos = repoPlato.ObtenerTodos();
            return View(pedido);
        }

        [HttpGet]
        public IActionResult Alta()
        {
            CargarCombos();
            return View(new Pedido());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Pedido pedido)
        {
            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
                ModelState.AddModelError("", "Debe agregar al menos un plato al pedido.");
            else if (pedido.Detalles.Any(d => d.Cantidad < 1))
                ModelState.AddModelError("", "La cantidad de cada plato debe ser mayor a cero.");

            if (ModelState.IsValid)
            {
                try
                {
                    int idCreado = repoPedido.Alta(pedido);
                    TempData["Mensaje"] = "Pedido registrado correctamente.";
                    return RedirectToAction(nameof(Detalle), new { id = idCreado });
                }
                catch (InvalidOperationException ex)
                {
                    // Plato inexistente o dado de baja
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Ocurrió un error al registrar el pedido: " + ex.Message;
                }
            }

            CargarCombos();
            return View(pedido);
        }

        public IActionResult Editar(int id)
        {
            var pedido = repoPedido.ObtenerPorId(id);
            if (pedido == null)
                return NotFound();

            if (pedido.estado != Pedido.Estado.Abierto)
            {
                TempData["Error"] = "Solo se pueden modificar pedidos abiertos.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            CargarCombos();
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Modificar(Pedido pedido)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View("Editar", pedido);
            }

            if (repoPedido.ModificarPedido(pedido))
                TempData["Mensaje"] = "Pedido actualizado correctamente.";
            else
                TempData["Error"] = "No se pudo modificar el pedido: no existe o ya no está abierto.";

            return RedirectToAction(nameof(Detalle), new { id = pedido.IdPedido });
        }

        // Cancela el pedido (el estado pasa a 'Cancelado')
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baja(int id)
        {
            if (repoPedido.Baja(id))
                TempData["Mensaje"] = "Pedido cancelado correctamente.";
            else
                TempData["Error"] = "No se pudo cancelar el pedido: no existe o ya no está abierto.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarDetalle(DetallePedido detalle)
        {
            if (!PedidoAbierto(detalle.IdPedido))
                return RedirectToAction(nameof(Detalle), new { id = detalle.IdPedido });

            if (detalle.Cantidad < 1)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction(nameof(Detalle), new { id = detalle.IdPedido });
            }

            try
            {
                repoDetalle.Alta(detalle);
                TempData["Mensaje"] = "Plato agregado al pedido.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { id = detalle.IdPedido });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModificarCantidad(int idDetallePedido, int idPedido, int cantidad)
        {
            if (!PedidoAbierto(idPedido))
                return RedirectToAction(nameof(Detalle), new { id = idPedido });

            if (cantidad < 1)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction(nameof(Detalle), new { id = idPedido });
            }

            try
            {
                repoDetalle.ModificarCantidad(idDetallePedido, cantidad);
                TempData["Mensaje"] = "Cantidad actualizada.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { id = idPedido });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BajaDetalle(int idDetallePedido, int idPedido)
        {
            if (!PedidoAbierto(idPedido))
                return RedirectToAction(nameof(Detalle), new { id = idPedido });

            try
            {
                repoDetalle.Baja(idDetallePedido);
                TempData["Mensaje"] = "Plato quitado del pedido.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detalle), new { id = idPedido });
        }

        // Los renglones solo se pueden tocar mientras el pedido está abierto
        private bool PedidoAbierto(int idPedido)
        {
            var pedido = repoPedido.ObtenerPorId(idPedido);
            if (pedido == null || pedido.estado != Pedido.Estado.Abierto)
            {
                TempData["Error"] = "El pedido no existe o ya no está abierto.";
                return false;
            }
            return true;
        }

        private void CargarCombos()
        {
            ViewBag.Mesas = repoMesa.ObtenerLista(1, 100);
            ViewBag.Empleados = repoEmpleado.ObtenerLista(1, 100);
            ViewBag.Platos = repoPlato.ObtenerTodos();
        }
    }
}
