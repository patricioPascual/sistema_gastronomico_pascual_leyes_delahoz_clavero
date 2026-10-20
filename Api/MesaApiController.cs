using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers.Api
{
    [Route("api/salon")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "2")]
    public class MesaApiController : ControllerBase
    {
        private readonly RepositorioMesa _repoMesa;
        private readonly RepositorioPedido _repoPedido;

        public MesaApiController(RepositorioMesa repoMesa, RepositorioPedido repoPedido)
        {

            _repoMesa = repoMesa;
            _repoPedido = repoPedido;

        }

        [HttpGet("estado-salon")]
        public IActionResult ObtenerEstadoSalon()
        {
            var todas = _repoMesa.ObtenerTodosConOcupacion();

            var mesas = todas.Where(m => m.Tipo == "Mesa").OrderBy(m => m.Numero).ToList();
            var barra = todas.Where(m => m.Tipo == "Barra").OrderBy(m => m.Numero).ToList();

            var pendientes = _repoPedido.ObtenerPedidosConDetallesEnMarcha().Select(p => new
            {
                idPedido = p.IdPedido,
                mesaNumero = p.Mesa != null ? p.Mesa.Numero : 0,
                empleadoNombre = p.Empleado != null ? $"{p.Empleado.Apellido}, {p.Empleado.Nombre}" : "Sin asignar",
                fechaHora = p.FechaHora.ToString("o"),
                cerrado = p.estado == Pedido.Estado.Pagado,
                detalles = p.Detalles.Select(d => new
                {
                    idDetallePedido = d.IdDetallePedido,
                    cantidad = d.Cantidad,
                    nombrePlato = d.Plato != null ? d.Plato.Nombre : "Plato",
                    fechaHora = d.FechaHora.ToString("o")
                }).ToList()
            }).ToList();


            return Ok(new { mesas, barra, pendientes });
        }
    }
}


