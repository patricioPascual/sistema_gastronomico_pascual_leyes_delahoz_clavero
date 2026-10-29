using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize(Roles = RolesSistema.Todos)]
    public class PedidoController : Controller
    {
        private readonly RepositorioPedido repoPedido;
        private readonly RepositorioDetallePedido repoDetalle;
        private readonly RepositorioMesa repoMesa;
        private readonly RepositorioEmpleado repoEmpleado;
        private readonly RepositorioPlato repoPlato;
        private readonly ILogger<PedidoController> logger;

        public PedidoController(RepositorioPedido repoPedido, RepositorioDetallePedido repoDetalle,
            RepositorioMesa repoMesa, RepositorioEmpleado repoEmpleado, RepositorioPlato repoPlato,
            ILogger<PedidoController> logger)
        {
            this.repoPedido = repoPedido;
            this.repoDetalle = repoDetalle;
            this.repoMesa = repoMesa;
            this.repoEmpleado = repoEmpleado;
            this.repoPlato = repoPlato;
            this.logger = logger;
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
                logger.LogError(ex, "Error al cargar el listado de pedidos");
                TempData["Error"] = "Ocurrió un error al cargar el listado de pedidos.";
                return View(new List<Pedido>());
            }
        }

        public IActionResult Detalle(int id)
        {
            var pedido = repoPedido.ObtenerPorId(id);
            if (pedido == null)
                return NotFound();

            // Las acciones de "Liberar mesa" / "Abrir nuevo pedido" solo se
            // muestran si ESTE pedido es el último pagado de la mesa (y la mesa
            // sigue con el flag de pendiente de liberar prendido). Si ya se abrió
            // un pedido nuevo en esa mesa, este pedido quedó viejo y no debe
            // ofrecer liberar nada.
            bool mostrarAccionesLiberar = pedido.estado == Pedido.Estado.Pagado
                && pedido.Mesa != null
                && pedido.Mesa.Estado
                && repoPedido.ObtenerUltimoPedidoPagadoId(pedido.IdMesa) == pedido.IdPedido;

            ViewBag.MostrarAccionesLiberar = mostrarAccionesLiberar;

            return View(pedido);
        }

        // idMesa llega desde el Salón cuando se hace click en una mesa/puesto libre,
        // para preseleccionar la mesa en el formulario.
        [HttpGet]
        public IActionResult Alta(int? idMesa)
        {
            CargarCombos();
            var pedido = new Pedido
            {
                // El empleado del pedido es siempre el usuario logueado: no se elige
                // de un <select>, se toma del claim "IdEmpleado" que quedó en la
                // cookie al loguearse (ver ClaimsHelper.ConstruirClaims).
                IdEmpleado = ObtenerIdEmpleadoLogueado()
            };
            ViewBag.NombreEmpleadoLogueado = ObtenerNombreEmpleadoLogueado();

            if (idMesa.HasValue)
            {
                var mesa = repoMesa.ObtenerPorId(idMesa.Value);
                if (mesa != null)
                {
                    pedido.IdMesa = mesa.IdMesa;
                    ViewBag.MesaSeleccionada = mesa; // la vista la muestra como texto, sin <select>
                }
                // si el id no existe más (mesa borrada, link viejo), se cae al <select> normal
            }

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(Pedido pedido)
        {
            // El mozo del pedido es SIEMPRE el usuario logueado: se pisa lo que venga del formulario
            // (el hidden se puede manipular) y por eso se saca IdEmpleado de la validación del modelo.
            pedido.IdEmpleado = ObtenerIdEmpleadoLogueado();
            ModelState.Remove(nameof(Pedido.IdEmpleado));

            // Si el claim no existe o no es numérico, ObtenerIdEmpleadoLogueado devuelve 0:
            // se corta acá en lugar de dejar que falle la FK en la base.
            if (pedido.IdEmpleado <= 0)
            {
                TempData["Error"] = "No se pudo identificar al empleado logueado. Volvé a iniciar sesión.";
                return RedirectToAction("Index", "Login");
            }

            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
                ModelState.AddModelError("", "Debe agregar al menos un plato al pedido.");
            else if (pedido.Detalles.Any(d => d.Cantidad < 1 || d.Cantidad > 99))
                ModelState.AddModelError("", "La cantidad de cada plato debe estar entre 1 y 99.");

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
                    // Errores de negocio lanzados a propósito por el repositorio (stock insuficiente,
                    // plato inactivo): el mensaje está pensado para el usuario.
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error al registrar el pedido de la mesa {IdMesa}", pedido.IdMesa);
                    TempData["Error"] = "Ocurrió un error inesperado al registrar el pedido. Intentá nuevamente.";
                }
            }

            CargarCombos();
            ViewBag.NombreEmpleadoLogueado = ObtenerNombreEmpleadoLogueado();
            if (pedido.IdMesa > 0)
                ViewBag.MesaSeleccionada = repoMesa.ObtenerPorId(pedido.IdMesa); // para que, si falla la validación, no se pierda el texto 
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
            int? idEmpleadoOriginal = repoPedido.ObtenerIdEmpleado(pedido.IdPedido);
            if (idEmpleadoOriginal == null)
                return NotFound();

            // Reasignar el pedido a otro empleado es una acción de administrador.
            // Para cualquier otro rol se ignora lo que venga en el formulario y se conserva el mozo original.
            if (!User.IsInRole(RolesSistema.Administrador))
            {
                pedido.IdEmpleado = idEmpleadoOriginal.Value;
                ModelState.Remove(nameof(Pedido.IdEmpleado));
            }
            else if (pedido.IdEmpleado != idEmpleadoOriginal.Value)
            {
                var empleado = repoEmpleado.ObtenerPorId(pedido.IdEmpleado);
                if (empleado == null || !empleado.Activo)
                    ModelState.AddModelError(nameof(Pedido.IdEmpleado), "El empleado seleccionado no existe o está dado de baja.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View("Editar", pedido);
            }

            if (repoPedido.Modificar(pedido))
                TempData["Mensaje"] = "Pedido actualizado correctamente.";
            else
                TempData["Error"] = "No se pudo modificar el pedido: no existe o ya no está abierto.";

            return RedirectToAction(nameof(Detalle), new { id = pedido.IdPedido });
        }

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

            if (detalle.Cantidad < 1 || detalle.Cantidad > 99)
            {
                TempData["Error"] = "La cantidad debe estar entre 1 y 99.";
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

            if (cantidad < 1 || cantidad > 99)
            {
                TempData["Error"] = "La cantidad debe estar entre 1 y 99.";
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

        // El empleado logueado se coloca solo en el pedido
        private int ObtenerIdEmpleadoLogueado()
        {
            var idEmpleadoClaim = User.FindFirstValue("IdEmpleado");
            int.TryParse(idEmpleadoClaim, out var idEmpleado);
            return idEmpleado;
        }

        private string ObtenerNombreEmpleadoLogueado()
        {
            return User.FindFirstValue("NombreCompleto") ?? "";
        }

        [HttpPost]
        public IActionResult DespacharPedido(int idDetallePedido, int idPedido)
        {
            bool exito = repoDetalle.CambiarEstadoDetalle(idDetallePedido, DetallePedido.Estado.Despachado);

            if (!exito)
            {
                TempData["Error"] = "No se pudo actualizar el estado del plato.";
            }
            else
            {
                TempData["Exito"] = "Plato marcado como despachado.";
            }


            return RedirectToAction("Detalle", new { id = idPedido });
        }


        [HttpPost]
        public IActionResult CerrarMesa(int idPedido)
        {
            bool exito = repoPedido.CerrarYPagarPedido(idPedido);

            if (exito)
            {
                TempData["Exito"] = "La mesa fue cobrada y liberada con éxito.";
            }
            else
            {
                TempData["Error"] = "No se pudo procesar el cierre del pedido.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LiberarMesa(int idMesa)
        {
            bool exito = repoMesa.LiberarMesa(idMesa);

            TempData[exito ? "Mensaje" : "Error"] = exito
                ? "La mesa fue liberada y está disponible para un nuevo pedido."
                : "No se pudo liberar la mesa.";

            return RedirectToAction("Salon", "Mesa");
        }
    }
}