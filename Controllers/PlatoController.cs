using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class PlatoController : Controller
    {
        private readonly RepositorioPlato _repositorioPlato;
        private readonly RepositorioDetalleReceta _repositorioDetalleReceta;
        private readonly RepositorioCategoria _repositorioCategoria;

        public PlatoController(
            RepositorioPlato repositorioPlato,
            RepositorioDetalleReceta repositorioDetalleReceta,
            RepositorioCategoria repositorioCategoria)
        {
            _repositorioPlato = repositorioPlato;
            _repositorioDetalleReceta = repositorioDetalleReceta;
            _repositorioCategoria = repositorioCategoria;
        }

        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            int totalRegistros = _repositorioPlato.ObtenerTotalRegistros();

            var platos = _repositorioPlato.ObtenerLista(pagina, tamPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);

            return View(platos);
        }

        public IActionResult Crear()
        {
            var vm = new PlatoFormViewModel
            {
                Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }

            try
            {
                vm.Plato.Estado = true;

                int idPlato = _repositorioPlato.Alta(vm.Plato);

                if (vm.Receta != null && idPlato > 0)
                {
                    var detalles = vm.Receta
                        .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                        .Select(r => new DetalleReceta
                        {
                            IdPlato = idPlato,
                            IdProducto = r.IdProducto,
                            CantidadRequerida = r.CantidadRequerida
                        })
                        .ToList();

                    _repositorioDetalleReceta.GuardarReceta(idPlato, detalles);
                }

                TempData["Exito"] = "El plato se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar el plato: " + ex.Message;
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var plato = _repositorioPlato.ObtenerPorId(id);
                if (plato != null)
                {
                    if (!plato.Estado)
                    {
                        TempData["Error"] = "El plato ya se encuentra inactivo.";
                    }
                    else
                    {
                        _repositorioPlato.Baja(id);
                        TempData["Exito"] = "El plato se dio de baja correctamente.";
                    }
                }
                else
                {
                    TempData["Error"] = "No se pudo encontrar el plato.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al intentar eliminar el plato: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Editar(int id)
        {
            var plato = _repositorioPlato.ObtenerPorId(id);

            if (plato == null)
            {
                TempData["Error"] = "El plato que intenta modificar no existe.";
                return RedirectToAction(nameof(Index));
            }

            var receta = _repositorioDetalleReceta.ObtenerPorPlato(id);

            var vm = new PlatoFormViewModel
            {
                Plato = plato,
                Receta = receta.Select(d => new DetalleRecetaFormItem
                {
                    IdDetalleReceta = d.IdDetalleReceta,
                    IdProducto = d.IdProducto,
                    CantidadRequerida = d.CantidadRequerida,
                    NombreProducto = d.Producto?.Nombre,
                    UnidadMedida = d.Producto?.Unidad_medida
                }).ToList(),
                Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }

            try
            {
                var platoExistente = _repositorioPlato.ObtenerPorId(id);
                if (platoExistente == null)
                {
                    TempData["Error"] = "El plato no existe.";
                    return RedirectToAction(nameof(Index));
                }

                platoExistente.Nombre = vm.Plato.Nombre;
                platoExistente.PrecioVenta = vm.Plato.PrecioVenta;
                platoExistente.IdCategoria = vm.Plato.IdCategoria;

                _repositorioPlato.Modificar(platoExistente);

                var nuevosDetalles = new List<DetalleReceta>();
                if (vm.Receta != null)
                {
                    nuevosDetalles = vm.Receta
                        .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                        .Select(r => new DetalleReceta
                        {
                            IdPlato = id,
                            IdProducto = r.IdProducto,
                            CantidadRequerida = r.CantidadRequerida
                        })
                        .ToList();
                }

                _repositorioDetalleReceta.GuardarReceta(id, nuevosDetalles);

                TempData["Exito"] = "El plato se modificó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al modificar el plato: " + ex.Message;
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }
        }
    }
}