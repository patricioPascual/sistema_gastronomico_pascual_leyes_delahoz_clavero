using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class DetalleRecetaController : ControllerBase
    {
        private readonly IRepositorioDetalleReceta repositorioDetalleReceta;

        public DetalleRecetaController(IRepositorioDetalleReceta repositorioDetalleReceta)
        {
            this.repositorioDetalleReceta = repositorioDetalleReceta;
        }

        [HttpPost]
        public ActionResult<DetalleReceta> Alta(DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = repositorioDetalleReceta.Alta(detalle);
            if (id <= 0)
            {
                return StatusCode(500, "No se pudo dar de alta el detalle de receta.");
            }

            detalle.IdDetalleReceta = id;
            return Ok(detalle);
        }

        [HttpPost]
        public IActionResult Modificar(int id, DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            detalle.IdDetalleReceta = id;
            var filasAfectadas = repositorioDetalleReceta.Modificar(detalle);
            if (filasAfectadas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var filasAfectadas = repositorioDetalleReceta.Eliminar(id);
            if (filasAfectadas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
