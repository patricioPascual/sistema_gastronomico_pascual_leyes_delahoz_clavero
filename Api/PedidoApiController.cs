using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers.Api
{
    // API de pedidos para la SPA del salón. Misma lógica de negocio que PedidoController (MVC),
    // pero devolviendo JSON: todos los errores salen como { mensaje = "..." } porque es lo que
    // lee la SPA (data.mensaje).
    [Route("api/pedidos")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesSistema.Mozo)]
    public class PedidoApiController : ControllerBase
    {
        private readonly RepositorioPedido _repoPedido;
        private readonly RepositorioDetallePedido _repoDetalle;
        private readonly RepositorioMesa _repoMesa;
        private readonly RepositorioPlato _repoPlato;
        private readonly ILogger<PedidoApiController> _logger;

        public PedidoApiController(
            RepositorioPedido repoPedido,
            RepositorioDetallePedido repoDetalle,
            RepositorioMesa repoMesa,
            RepositorioPlato repoPlato,
            ILogger<PedidoApiController> logger)
        {
            _repoPedido = repoPedido;
            _repoDetalle = repoDetalle;
            _repoMesa = repoMesa;
            _repoPlato = repoPlato;
            _logger = logger;
        }

        // GET api/pedidos/{idPedido}
        // Detalle completo de un pedido (todos los platos, con su estado) y su total. La SPA lo usa
        // para mostrar lo ya pedido cuando se abre una mesa ocupada.
        [HttpGet("{idPedido:int}")]
        public IActionResult Obtener(int idPedido)
        {
            var pedido = _repoPedido.ObtenerPorId(idPedido);
            if (pedido == null)
                return NotFound(new { mensaje = "El pedido no existe." });

            return Ok(new
            {
                idPedido = pedido.IdPedido,
                mesaNumero = pedido.Mesa?.Numero ?? 0,
                estado = pedido.estado.ToString(),
                total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                detalles = pedido.Detalles
                    .OrderBy(d => d.FechaHora)
                    .Select(d => new
                    {
                        idDetallePedido = d.IdDetallePedido,
                        nombrePlato = d.Plato != null ? d.Plato.Nombre : "Plato",
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioUnitario,
                        subtotal = d.Cantidad * d.PrecioUnitario,
                        despachado = d.estado == DetallePedido.Estado.Despachado
                    })
                    .ToList()
            });
        }

        // POST api/pedidos/alta
        // Body: { "idMesa": 3, "detalles": [ { "idPlato": 1, "cantidad": 2 } ] }
        // - Mesa libre (o pendiente de liberar): crea un pedido nuevo (stock + pedido en UNA transacción
        //   dentro de RepositorioPedido.Alta: commit si alcanza el stock, rollback si no).
        // - Mesa con pedido Abierto: agrega los platos a ese pedido (segunda ronda).
        // El mozo sale del claim "IdEmpleado" del usuario logueado, no del body.
        [HttpPost("alta")]
        public IActionResult Alta([FromBody] AltaPedidoRequest request)
        {
            if (request.IdMesa <= 0)
                return BadRequest(new { mensaje = "Mesa inválida." });

            if (request.Detalles == null || request.Detalles.Count == 0)
                return BadRequest(new { mensaje = "Debe agregar al menos un plato al pedido." });

            if (request.Detalles.Any(d => d.IdPlato <= 0 || d.Cantidad < 1))
                return BadRequest(new { mensaje = "La cantidad de cada plato debe ser mayor a cero." });

            int? idEmpleado = ObtenerIdEmpleado();
            if (idEmpleado == null)
                return Unauthorized(new { mensaje = "El usuario no tiene un empleado asociado." });

            var mesa = _repoMesa.ObtenerPorId(request.IdMesa);
            if (mesa == null)
                return NotFound(new { mensaje = "La mesa no existe." });

            // Si el mismo plato viene en dos líneas, se unifican para validar el stock una sola vez.
            var lineas = request.Detalles
                .GroupBy(d => d.IdPlato)
                .Select(g => new DetallePedidoRequest { IdPlato = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToList();

            try
            {
                var abierto = _repoPedido.ObtenerAbiertos().FirstOrDefault(p => p.IdMesa == mesa.IdMesa);

                if (abierto != null)
                    return AgregarAPedidoAbierto(abierto.IdPedido, lineas);

                var pedido = new Pedido
                {
                    IdMesa = mesa.IdMesa,
                    IdEmpleado = idEmpleado.Value,
                    Detalles = lineas.Select(l => new DetallePedido
                    {
                        IdPlato = l.IdPlato,
                        Cantidad = l.Cantidad
                    }).ToList()
                };

                int idCreado = _repoPedido.Alta(pedido);
                decimal total = _repoPedido.ObtenerPorId(idCreado)?.Total ?? 0;

                return StatusCode(StatusCodes.Status201Created, new
                {
                    mensaje = "Pedido registrado correctamente.",
                    idPedido = idCreado,
                    total,
                    agregadoAPedidoExistente = false
                });
            }
            catch (InvalidOperationException ex)
            {
                // Stock insuficiente / plato inactivo: error de negocio, el mensaje es apto para mostrar.
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el pedido de la mesa {IdMesa}", request.IdMesa);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al registrar el pedido." });
            }
        }

        // PUT api/pedidos/detalle/{idDetallePedido}
        [HttpPut("detalle/{idDetallePedido:int}")]
        public IActionResult ModificarCantidadDetalle(int idDetallePedido, [FromBody] ModificarCantidadRequest request)
        {
            if (request.Cantidad < 1)
                return BadRequest(new { mensaje = "La cantidad debe ser mayor a cero." });

            var detalle = _repoDetalle.ObtenerPorId(idDetallePedido);
            if (detalle == null)
                return NotFound(new { mensaje = "El plato del pedido no existe." });

            // Bloquear si ya esta despachado
            if (detalle.estado == DetallePedido.Estado.Despachado)
                return Conflict(new { mensaje = "No se puede modificar la cantidad de un plato que ya fue despachado." });

            var pedido = _repoPedido.ObtenerPorId(detalle.IdPedido);
            if (pedido == null || pedido.estado != Pedido.Estado.Abierto)
                return Conflict(new { mensaje = "El pedido no existe o ya no está abierto." });

            try
            {
                _repoDetalle.ModificarCantidad(idDetallePedido, request.Cantidad);

                return Ok(new
                {
                    mensaje = "Cantidad actualizada.",
                    idPedido = detalle.IdPedido,
                    total = _repoPedido.ObtenerPorId(detalle.IdPedido)?.Total ?? 0
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar la cantidad del detalle {IdDetallePedido}", idDetallePedido);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al modificar la cantidad." });
            }
        }

        // DELETE api/pedidos/detalle/{idDetallePedido}
        [HttpDelete("detalle/{idDetallePedido:int}")]
        public IActionResult EliminarDetalle(int idDetallePedido)
        {
            var detalle = _repoDetalle.ObtenerPorId(idDetallePedido);
            if (detalle == null)
                return NotFound(new { mensaje = "El plato del pedido no existe." });

            // Bloquear si ya esta despachado
            if (detalle.estado == DetallePedido.Estado.Despachado)
                return Conflict(new { mensaje = "No se puede quitar un plato que ya fue despachado o consumido." });

            var pedido = _repoPedido.ObtenerPorId(detalle.IdPedido);
            if (pedido == null || pedido.estado != Pedido.Estado.Abierto)
                return Conflict(new { mensaje = "El pedido no existe o ya no está abierto." });

            try
            {
                _repoDetalle.Baja(idDetallePedido);

                return Ok(new
                {
                    mensaje = "Plato quitado del pedido.",
                    idPedido = detalle.IdPedido,
                    total = _repoPedido.ObtenerPorId(detalle.IdPedido)?.Total ?? 0
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el detalle {IdDetallePedido}", idDetallePedido);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al quitar el plato." });
            }
        }

        // DELETE api/pedidos/{idPedido}
        [HttpDelete("{idPedido:int}")]
        public IActionResult Baja(int idPedido)
        {
            bool exito = _repoPedido.Baja(idPedido);

            if (!exito)
                return Conflict(new { mensaje = "No se pudo cancelar el pedido: no existe o ya no está abierto." });

            return Ok(new { mensaje = "Pedido cancelado correctamente." });
        }

        // POST api/pedidos/detalle/{idDetallePedido}/despachar
        [HttpPost("detalle/{idDetallePedido:int}/despachar")]
        public IActionResult DespacharDetalle(int idDetallePedido)
        {
            var detalle = _repoDetalle.ObtenerPorId(idDetallePedido);
            if (detalle == null)
                return NotFound(new { mensaje = "El plato del pedido no existe." });

            if (detalle.estado == DetallePedido.Estado.Despachado)
                return Ok(new { mensaje = "El plato ya estaba despachado." });

            try
            {
                bool exito = _repoDetalle.CambiarEstadoDetalle(idDetallePedido, DetallePedido.Estado.Despachado);
                if (!exito)
                    return Conflict(new { mensaje = "No se pudo actualizar el estado del plato." });

                return Ok(new { mensaje = "Plato marcado como despachado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al despachar el detalle {IdDetallePedido}", idDetallePedido);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al despachar el plato." });
            }
        }

        // POST api/pedidos/{idPedido}/cerrar
        // Cobra el pedido: lo pasa a Pagado, recalcula el total y deja la mesa pendiente de liberar.
        [HttpPost("{idPedido:int}/cerrar")]
        public IActionResult Cerrar(int idPedido)
        {
            var pedido = _repoPedido.ObtenerPorId(idPedido);
            if (pedido == null)
                return NotFound(new { mensaje = "El pedido no existe." });

            if (pedido.estado != Pedido.Estado.Abierto)
                return Conflict(new { mensaje = "El pedido ya no está abierto." });

            try
            {
                if (!_repoPedido.CerrarYPagarPedido(idPedido))
                    return Conflict(new { mensaje = "No se pudo procesar el cierre del pedido." });

                return Ok(new
                {
                    mensaje = "La mesa fue cobrada con éxito.",
                    idPedido,
                    total = _repoPedido.ObtenerPorId(idPedido)?.Total ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar el pedido {IdPedido}", idPedido);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al cerrar el pedido." });
            }
        }

        // POST api/pedidos/mesa/{idMesa}/liberar
        // Libera una mesa que quedó pendiente después de cobrarse.
        [HttpPost("mesa/{idMesa:int}/liberar")]
        public IActionResult LiberarMesa(int idMesa)
        {
            var mesa = _repoMesa.ObtenerPorId(idMesa);
            if (mesa == null)
                return NotFound(new { mensaje = "La mesa no existe." });

            if (!mesa.Estado)
                return Ok(new { mensaje = "La mesa no tenía nada pendiente de liberar." });

            try
            {
                if (!_repoMesa.LiberarMesa(idMesa))
                    return Conflict(new { mensaje = "No se pudo liberar la mesa." });

                return Ok(new { mensaje = "La mesa fue liberada y está disponible para un nuevo pedido." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar la mesa {IdMesa}", idMesa);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = "Ocurrió un error al liberar la mesa." });
            }
        }

        // ---------- Auxiliares ----------

        // Segunda ronda en una mesa ocupada: se valida el stock de TODAS las líneas antes de tocar
        // nada, y recién después se agregan. Cada línea usa RepositorioDetallePedido.Alta (con su
        // propia transacción), por eso la validación previa es la que evita quedar a medias.
        private IActionResult AgregarAPedidoAbierto(int idPedido, List<DetallePedidoRequest> lineas)
        {
            foreach (var linea in lineas)
            {
                var plato = _repoPlato.ObtenerPorId(linea.IdPlato);
                if (plato == null || !plato.Estado)
                    throw new InvalidOperationException("Uno de los platos seleccionados no existe o no está activo.");

                var stock = _repoDetalle.ObtenerStockDisponiblePlato(linea.IdPlato);
                if (linea.Cantidad > stock.CantidadDisponible)
                {
                    string faltante = !string.IsNullOrEmpty(stock.InsumoFaltante)
                        ? $" (Falta: {stock.InsumoFaltante})"
                        : "";
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{plato.Nombre}'{faltante}. Quedan {stock.CantidadDisponible} porciones.");
                }
            }

            foreach (var linea in lineas)
            {
                _repoDetalle.Alta(new DetallePedido
                {
                    IdPedido = idPedido,
                    IdPlato = linea.IdPlato,
                    Cantidad = linea.Cantidad
                });
            }

            return Ok(new
            {
                mensaje = "Platos agregados al pedido.",
                idPedido,
                total = _repoPedido.ObtenerPorId(idPedido)?.Total ?? 0,
                agregadoAPedidoExistente = true
            });
        }

        private int? ObtenerIdEmpleado()
        {
            var valor = User.FindFirst("IdEmpleado")?.Value;
            return int.TryParse(valor, out int id) && id > 0 ? id : null;
        }
    }

    // DTOs de entrada de la API (no son entidades de la base).
    public class AltaPedidoRequest
    {
        public int IdMesa { get; set; }
        public List<DetallePedidoRequest> Detalles { get; set; } = new();
    }

    public class DetallePedidoRequest
    {
        public int IdPlato { get; set; }
        public int Cantidad { get; set; }
    }

    public class ModificarCantidadRequest
    {
        public int Cantidad { get; set; }
    }
}
