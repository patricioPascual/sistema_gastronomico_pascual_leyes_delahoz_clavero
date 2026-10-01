using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleRecetaController : ControllerBase
    {
        private readonly RepositorioDetalleReceta _repositorioDetalleReceta;

        public DetalleRecetaController(RepositorioDetalleReceta repositorioDetalleReceta)
        {
            _repositorioDetalleReceta = repositorioDetalleReceta;
        }

        [HttpPost]
        public ActionResult<DetalleReceta> Alta(DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int id = _repositorioDetalleReceta.Alta(detalle);

            if (id <= 0)
            {
                return StatusCode(500, "No se pudo dar de alta el detalle de receta.");
            }

            return Ok(detalle);
        }

        [HttpPost("modificar/{id}")]
        public IActionResult Modificar(int id, DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            detalle.IdDetalleReceta = id;
            int filas = _repositorioDetalleReceta.Modificar(detalle);

            if (filas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("eliminar/{id}")]
        public IActionResult Eliminar(int id)
        {
            int filas = _repositorioDetalleReceta.Eliminar(id);
            if (filas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}