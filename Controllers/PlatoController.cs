using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class PlatoController : ControllerBase
    {
        private readonly IRepositorioPlato repositorioPlato;
        private readonly IRepositorioDetalleReceta repositorioDetalleReceta;

        public PlatoController(IRepositorioPlato repositorioPlato, IRepositorioDetalleReceta repositorioDetalleReceta)
        {
            this.repositorioPlato = repositorioPlato;
            this.repositorioDetalleReceta = repositorioDetalleReceta;
        }

        [HttpGet]
        public ActionResult<IList<Plato>> ObtenerTodos()
        {
            return Ok(repositorioPlato.ObtenerTodos());
        }

        [HttpGet("categoria/{idCategoria}")]
        public ActionResult<IList<Plato>> ObtenerPorCategoria(int idCategoria)
        {
            return Ok(repositorioPlato.ObtenerPorCategoria(idCategoria));
        }

        [HttpGet]
        public ActionResult<IList<Plato>> Buscar([FromQuery] string q)
        {
            return Ok(repositorioPlato.Buscar(q ?? ""));
        }

        [HttpGet]
        public ActionResult<Plato> ObtenerPorId(int id)
        {
            var plato = repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                return NotFound();
            }

            return Ok(plato);
        }

        [HttpGet]
        public ActionResult<IList<DetalleReceta>> ObtenerReceta(int id)
        {
            var plato = repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                return NotFound();
            }

            return Ok(repositorioDetalleReceta.ObtenerPorPlato(id));
        }

        [HttpPost]
        public ActionResult<Plato> Alta([FromBody] Plato plato)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = repositorioPlato.Alta(plato);
            if (id <= 0)
            {
                return StatusCode(500, "No se pudo dar de alta el plato.");
            }

            plato.IdPlato = id;
            return CreatedAtAction(nameof(ObtenerPorId), new { id = plato.IdPlato }, plato);
        }

        [HttpPost]
        public IActionResult Modificar(int id, [FromBody] Plato plato)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            plato.IdPlato = id;
            var filasAfectadas = repositorioPlato.Modificar(plato);
            if (filasAfectadas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost]
        public IActionResult GuardarReceta(int id, List<DetalleReceta> detalles)
        {
            var plato = repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                return NotFound();
            }

            foreach (var detalle in detalles)
            {
                detalle.IdPlato = id;
            }

            repositorioDetalleReceta.GuardarReceta(id, detalles);
            return NoContent();
        }

        [HttpPost]
        public IActionResult Baja(int id)
        {
            var filasAfectadas = repositorioPlato.Baja(id);
            if (filasAfectadas <= 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
