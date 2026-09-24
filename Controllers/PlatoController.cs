using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class PlatoController : Controller
    {
        private readonly IRepositorioPlato repositorioPlato;
        private readonly IRepositorioDetalleReceta repositorioDetalleReceta;
        private readonly RepositorioProducto repositorioProducto;
        private readonly RepositorioCategoria repositorioCategoria;

        public PlatoController(
            IRepositorioPlato repositorioPlato,
            IRepositorioDetalleReceta repositorioDetalleReceta,
            RepositorioProducto repositorioProducto,
            RepositorioCategoria repositorioCategoria)
        {
            this.repositorioPlato = repositorioPlato;
            this.repositorioDetalleReceta = repositorioDetalleReceta;
            this.repositorioProducto = repositorioProducto;
            this.repositorioCategoria = repositorioCategoria;
        }

        public IActionResult Index()
        {
            var platos = repositorioPlato.ObtenerTodos();
            return View(platos);
        }

        public IActionResult Crear()
        {
            var vm = new PlatoFormViewModel
            {
                ProductosDisponibles = repositorioProducto.Buscar("").ToList(),
                Categorias = repositorioCategoria.ObtenerTodos().ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ProductosDisponibles = repositorioProducto.Buscar("").ToList();
                vm.Categorias = repositorioCategoria.ObtenerTodos().ToList();
                return View(vm);
            }

            vm.Plato.Estado = true;
            var idPlato = repositorioPlato.Alta(vm.Plato);

            var detalles = vm.Receta
                .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                .Select(r => new DetalleReceta
                {
                    IdPlato = idPlato,
                    IdProducto = r.IdProducto,
                    CantidadRequerida = r.CantidadRequerida
                })
                .ToList();

            repositorioDetalleReceta.GuardarReceta(idPlato, detalles);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var plato = repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                return NotFound();
            }

            var receta = repositorioDetalleReceta.ObtenerPorPlato(id);

            var vm = new PlatoFormViewModel
            {
                Plato = plato,
                Receta = receta.Select(d => new DetalleRecetaFormItem
                {
                    IdDetalleReceta = d.IdDetalleReceta,
                    IdProducto = d.IdProducto,
                    CantidadRequerida = d.CantidadRequerida
                }).ToList(),
                ProductosDisponibles = repositorioProducto.Buscar("").ToList(),
                Categorias = repositorioCategoria.ObtenerTodos().ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ProductosDisponibles = repositorioProducto.Buscar("").ToList();
                vm.Categorias = repositorioCategoria.ObtenerTodos().ToList();
                return View(vm);
            }

            vm.Plato.IdPlato = id;
            repositorioPlato.Modificar(vm.Plato);

            var detalles = vm.Receta
                .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                .Select(r => new DetalleReceta
                {
                    IdPlato = id,
                    IdProducto = r.IdProducto,
                    CantidadRequerida = r.CantidadRequerida
                })
                .ToList();

            repositorioDetalleReceta.GuardarReceta(id, detalles);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Eliminar(int id)
        {
            var plato = repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                return NotFound();
            }
            return View(plato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            repositorioPlato.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}