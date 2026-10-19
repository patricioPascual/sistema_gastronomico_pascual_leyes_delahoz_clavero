using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using System.Linq;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers.Api
{
    [Route("api/plato")]
    [ApiController]

    [Authorize(Roles = "1,2")]
    public class PlatoApiController : ControllerBase
    {
        private readonly RepositorioPlato _repositorioPlato;
        private readonly RepositorioDetalleReceta _repositorioDetalleReceta;

        public PlatoApiController(
            RepositorioPlato repositorioPlato,
            RepositorioDetalleReceta repositorioDetalleReceta)
        {
            _repositorioPlato = repositorioPlato;
            _repositorioDetalleReceta = repositorioDetalleReceta;
        }

        [HttpGet]
        public IActionResult ObtenerMenu()
        {
            var platos = _repositorioPlato.ObtenerLista(1, 1000)
                            .Where(p => p.Estado)
                            .Select(p => new
                            {
                                idPlato = p.IdPlato,
                                nombre = p.Nombre,
                                precioVenta = p.PrecioVenta,
                                idCategoria = p.IdCategoria,
                                categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría",
                                imagenUrl = $"/img/platos/plato_{p.IdPlato}.jpg"
                            })
                            .ToList();

            return Ok(platos);
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerDetalle(int id)
        {
            var plato = _repositorioPlato.ObtenerPorId(id);

            if (plato == null || !plato.Estado)
            {
                return NotFound(new { mensaje = "Plato no encontrado o inactivo." });
            }

            var receta = _repositorioDetalleReceta.ObtenerPorPlato(id)
                            .Select(r => new
                            {
                                nombreIngrediente = r.Producto?.Nombre,
                                cantidad = r.CantidadRequerida,
                                unidad = r.Producto?.Unidad_medida
                            })
                            .ToList();

            var detalleJson = new
            {
                idPlato = plato.IdPlato,
                nombre = plato.Nombre,
                precioVenta = plato.PrecioVenta,
                imagenUrl = $"/img/platos/plato_{plato.IdPlato}.jpg",
                ingredientes = receta
            };

            return Ok(detalleJson);
        }

        [HttpGet("buscar")]
        public IActionResult BuscarEnMenu([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<object>());
            }

            var platos = _repositorioPlato.BuscarPlato(q)
                            .Where(p => p.Estado)
                            .Select(p => new
                            {
                                idPlato = p.IdPlato,
                                nombre = p.Nombre,
                                precioVenta = p.PrecioVenta
                            })
                            .ToList();

            return Ok(platos);
        }
    }
}