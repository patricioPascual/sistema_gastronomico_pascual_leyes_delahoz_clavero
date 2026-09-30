using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleRecetaController : ControllerBase
    {
        private readonly GastronomiaContext _context;

        public DetalleRecetaController(GastronomiaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult<DetalleReceta> Alta(DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DetalleRecetas.Add(detalle);
            int filas = _context.SaveChanges();

            if (filas <= 0)
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

            var existente = _context.DetalleRecetas.Find(id);
            if (existente == null)
            {
                return NotFound();
            }

            existente.IdProducto = detalle.IdProducto;
            existente.CantidadRequerida = detalle.CantidadRequerida;

            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost("eliminar/{id}")]
        public IActionResult Eliminar(int id)
        {
            var detalle = _context.DetalleRecetas.Find(id);
            if (detalle == null)
            {
                return NotFound();
            }

            _context.DetalleRecetas.Remove(detalle);
            _context.SaveChanges();

            return NoContent();
        }
    }
}