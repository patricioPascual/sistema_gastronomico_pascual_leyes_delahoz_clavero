using Microsoft.AspNetCore.Mvc;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    [Authorize]
    public class PlatoController : Controller
    {
        private readonly RepositorioPlato _repositorioPlato;
        private readonly RepositorioDetalleReceta _repositorioDetalleReceta;
        private readonly RepositorioCategoria _repositorioCategoria;
        private readonly IWebHostEnvironment _host;

        public PlatoController(
            RepositorioPlato repositorioPlato,
            RepositorioDetalleReceta repositorioDetalleReceta,
            RepositorioCategoria repositorioCategoria,
            IWebHostEnvironment host)
        {
            _repositorioPlato = repositorioPlato;
            _repositorioDetalleReceta = repositorioDetalleReceta;
            _repositorioCategoria = repositorioCategoria;
            _host = host;
        }

        [Authorize(Roles = RolesSistema.Todos)]
        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            int totalRegistros = _repositorioPlato.ObtenerTotalRegistros();

            var platos = _repositorioPlato.ObtenerLista(pagina, tamPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);

            return View(platos);
        }

        [Authorize(Roles = RolesSistema.Todos)]
        public IActionResult Detalle(int id)
        {
            var plato = _repositorioPlato.ObtenerPorId(id);
            if (plato == null)
            {
                TempData["Error"] = "El plato que intenta ver no existe.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Receta = _repositorioDetalleReceta.ObtenerPorPlato(id);
            return View(plato);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        public IActionResult Crear()
        {
            var vm = new PlatoFormViewModel
            {
                Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList()
            };
            return View(vm);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }

            if (vm.ArchivoImagen != null && !ValidarImagen(vm.ArchivoImagen))
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
                    var detalles = vm.Receta.Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                        .Select(r => new DetalleReceta { IdPlato = idPlato, IdProducto = r.IdProducto, CantidadRequerida = r.CantidadRequerida }).ToList();
                    _repositorioDetalleReceta.GuardarReceta(idPlato, detalles);
                }

                if (vm.ArchivoImagen != null)
                {
                    await ProcesarImagenLocal(idPlato, vm.ArchivoImagen, false);

                    var platoCreado = _repositorioPlato.ObtenerPorId(idPlato);
                    if (platoCreado != null)
                    {
                        platoCreado.Imagen = $"/img/platos/plato_{idPlato}.jpg";
                        _repositorioPlato.Modificar(platoCreado);
                    }
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

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }

            if (vm.ArchivoImagen != null && !ValidarImagen(vm.ArchivoImagen))
            {
                vm.Categorias = _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
                return View(vm);
            }

            try
            {
                var platoExistente = _repositorioPlato.ObtenerPorId(id);
                if (platoExistente == null) return NotFound();

                platoExistente.Nombre = vm.Plato.Nombre;
                platoExistente.PrecioVenta = vm.Plato.PrecioVenta;
                platoExistente.IdCategoria = vm.Plato.IdCategoria;

                if (vm.EliminarImagen)
                {
                    platoExistente.Imagen = null;
                }
                else if (vm.ArchivoImagen != null)
                {
                    platoExistente.Imagen = $"/img/platos/plato_{id}.jpg";
                }

                _repositorioPlato.Modificar(platoExistente);

                var nuevosDetalles = vm.Receta != null
                    ? vm.Receta.Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0).Select(r => new DetalleReceta { IdPlato = id, IdProducto = r.IdProducto, CantidadRequerida = r.CantidadRequerida }).ToList()
                    : new List<DetalleReceta>();
                _repositorioDetalleReceta.GuardarReceta(id, nuevosDetalles);

                await ProcesarImagenLocal(id, vm.ArchivoImagen, vm.EliminarImagen);

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

        [Authorize(Roles = RolesSistema.Administrador)]
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

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpGet]
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

        private async Task ProcesarImagenLocal(int idPlato, IFormFile? archivo, bool eliminar)
        {
            string carpetaDestino = Path.Combine(_host.WebRootPath, "img", "platos");
            string rutaArchivo = Path.Combine(carpetaDestino, $"plato_{idPlato}.jpg");

            if (eliminar || archivo != null)
            {
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }
            }

            if (archivo != null && archivo.Length > 0)
            {
                if (!Directory.Exists(carpetaDestino))
                {
                    Directory.CreateDirectory(carpetaDestino);
                }

                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }
            }
        }

        [Authorize(Roles = RolesSistema.Todos)]
        [HttpGet]
        public IActionResult BuscarPlato(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var productos = _repositorioPlato.BuscarPlato(q);

            var resultado = productos.Select(p => new
            {
                id = p.IdPlato,
                texto = p.Nombre,
                precioCosto = p.PrecioVenta
            });

            return Json(resultado);
        }

        private bool ValidarImagen(IFormFile? archivo)
        {
            if (archivo == null)
            {
                return true;
            }

            long maxSizeBytes = 2 * 1024 * 1024;
            if (archivo.Length > maxSizeBytes)
            {
                TempData["Error"] = "La imagen no debe superar los 2 MB de peso.";
                return false;
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !extensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Solo se permiten imágenes con extensión .jpg, .jpeg, .png o .webp.";
                return false;
            }

            var mimeTypesPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!mimeTypesPermitidos.Contains(archivo.ContentType.ToLower()))
            {
                TempData["Error"] = "El archivo subido no es una imagen válida.";
                return false;
            }

            return true;
        }

    }

}