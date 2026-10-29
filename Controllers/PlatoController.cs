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
        private readonly RepositorioProducto _repositorioProducto;
        private readonly GastronomiaContext _context;
        private readonly IWebHostEnvironment _host;
        private readonly ILogger<PlatoController> _logger;

        public PlatoController(
            RepositorioPlato repositorioPlato,
            RepositorioDetalleReceta repositorioDetalleReceta,
            RepositorioCategoria repositorioCategoria,
            RepositorioProducto repositorioProducto,
            GastronomiaContext context,
            IWebHostEnvironment host,
            ILogger<PlatoController> logger)
        {
            _repositorioPlato = repositorioPlato;
            _repositorioDetalleReceta = repositorioDetalleReceta;
            _repositorioCategoria = repositorioCategoria;
            _repositorioProducto = repositorioProducto;
            _context = context;
            _host = host;
            _logger = logger;
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
                Categorias = ObtenerCategoriasActivas()
            };
            return View(vm);
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PlatoFormViewModel vm)
        {
            vm.Plato.Nombre = vm.Plato.Nombre?.Trim();

            if (!string.IsNullOrEmpty(vm.Plato.Nombre) && _repositorioPlato.ExisteNombreActivo(vm.Plato.Nombre))
                ModelState.AddModelError("Plato.Nombre", "Ya existe un plato activo con ese nombre.");

            var detalles = ValidarReceta(vm.Receta);

            if (!ModelState.IsValid)
                return VistaConErrores(vm);

            if (vm.ArchivoImagen != null && !ValidarImagen(vm.ArchivoImagen))
                return VistaConErrores(vm);

            // Plato + receta en una sola transacción: si falla la receta no queda un plato sin ingredientes.
            using var transaccion = _context.Database.BeginTransaction();
            int idPlato;
            try
            {
                vm.Plato.Estado = true;
                idPlato = _repositorioPlato.Alta(vm.Plato);
                _repositorioDetalleReceta.GuardarReceta(idPlato, detalles);
                transaccion.Commit();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                _logger.LogError(ex, "Error al registrar el plato {Nombre}", vm.Plato.Nombre);
                TempData["Error"] = "Ocurrió un error inesperado al registrar el plato. Intentá nuevamente.";
                return VistaConErrores(vm);
            }

            // La imagen se procesa después del commit: el archivo en disco no forma parte de la transacción.
            if (vm.ArchivoImagen != null)
            {
                try
                {
                    await ProcesarImagenLocal(idPlato, vm.ArchivoImagen, false);

                    var platoCreado = _repositorioPlato.ObtenerPorId(idPlato);
                    if (platoCreado != null)
                    {
                        platoCreado.Imagen = $"/img/platos/plato_{idPlato}.jpg";
                        _repositorioPlato.Modificar(platoCreado);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar la imagen del plato {IdPlato}", idPlato);
                    TempData["Error"] = "El plato se registró, pero no se pudo guardar la imagen. Podés subirla desde Editar.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Exito"] = "El plato se registró correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RolesSistema.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PlatoFormViewModel vm)
        {
            var platoExistente = _repositorioPlato.ObtenerPorId(id);
            if (platoExistente == null) return NotFound();

            vm.Plato.Nombre = vm.Plato.Nombre?.Trim();

            if (!string.IsNullOrEmpty(vm.Plato.Nombre) && _repositorioPlato.ExisteNombreActivo(vm.Plato.Nombre, id))
                ModelState.AddModelError("Plato.Nombre", "Ya existe otro plato activo con ese nombre.");

            var nuevosDetalles = ValidarReceta(vm.Receta);

            if (!ModelState.IsValid)
                return VistaConErrores(vm);

            if (vm.ArchivoImagen != null && !ValidarImagen(vm.ArchivoImagen))
                return VistaConErrores(vm);

            using var transaccion = _context.Database.BeginTransaction();
            try
            {
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

                if (vm.EliminarImagen)
                {
                    platoExistente.Imagen = null;
                }
                else if (vm.ArchivoImagen != null)
                {
                    platoExistente.Imagen = $"/img/platos/plato_{id}.jpg";
                }

                _repositorioPlato.Modificar(platoExistente);
                _repositorioDetalleReceta.GuardarReceta(id, nuevosDetalles);
                transaccion.Commit();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();
                _logger.LogError(ex, "Error al modificar el plato {IdPlato}", id);
                TempData["Error"] = "Ocurrió un error inesperado al modificar el plato. Intentá nuevamente.";
                return VistaConErrores(vm);
            }

            try
            {
                await ProcesarImagenLocal(id, vm.ArchivoImagen, vm.EliminarImagen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen del plato {IdPlato}", id);
                TempData["Error"] = "Los datos del plato se guardaron, pero no se pudo actualizar la imagen.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Exito"] = "El plato se modificó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RolesSistema.Administrador)]
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
                _logger.LogError(ex, "Error al dar de baja el plato {IdPlato}", id);
                TempData["Error"] = "Ocurrió un error inesperado al dar de baja el plato.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RolesSistema.Administrador)]
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
                Categorias = ObtenerCategoriasActivas()
            };

            return View(vm);
        }

        // Valida las líneas de la receta y devuelve las que se van a guardar.
        // Antes se descartaban en silencio las líneas con cantidad 0 y un producto repetido terminaba
        // en la excepción del índice uq_plato_producto. Ahora cada caso se informa al usuario.
        // Solo se ignoran las filas totalmente vacías (sin insumo y sin cantidad), que son las que
        // agrega la vista por defecto.
        private List<DetalleReceta> ValidarReceta(List<DetalleRecetaFormItem>? receta)
        {
            var detalles = new List<DetalleReceta>();
            var lineas = (receta ?? new List<DetalleRecetaFormItem>())
                .Where(r => r.IdProducto > 0 || r.CantidadRequerida != 0)
                .ToList();

            if (lineas.Count == 0)
            {
                ModelState.AddModelError("", "La receta debe tener al menos un ingrediente.");
                return detalles;
            }

            var productosVistos = new HashSet<int>();
            int numeroLinea = 0;

            foreach (var linea in lineas)
            {
                numeroLinea++;

                if (linea.IdProducto <= 0)
                {
                    ModelState.AddModelError("", $"Ingrediente {numeroLinea}: tiene cantidad pero no se eligió el insumo.");
                    continue;
                }

                var producto = _repositorioProducto.ObtenerPorId(linea.IdProducto);
                string nombre = producto?.Nombre ?? $"insumo #{linea.IdProducto}";

                if (producto == null || !producto.Estado)
                {
                    ModelState.AddModelError("", $"Ingrediente {numeroLinea}: '{nombre}' no existe o está dado de baja.");
                    continue;
                }

                if (!productosVistos.Add(linea.IdProducto))
                {
                    ModelState.AddModelError("", $"'{nombre}' está repetido en la receta. Unificá las cantidades en una sola línea.");
                    continue;
                }

                if (linea.CantidadRequerida < ReglasValidacion.CantidadRecetaMinima
                    || linea.CantidadRequerida > ReglasValidacion.CantidadRecetaMaxima)
                {
                    ModelState.AddModelError("", $"'{nombre}': la cantidad debe estar entre 0,001 y 100.000 {producto.Unidad_medida}.");
                    continue;
                }

                detalles.Add(new DetalleReceta
                {
                    IdProducto = linea.IdProducto,
                    CantidadRequerida = linea.CantidadRequerida
                });
            }

            return detalles;
        }

        // Al volver a mostrar el formulario con errores hay que recargar las categorías y
        // completar nombre y unidad de cada insumo: el POST solo trae IdProducto y cantidad,
        // y sin esto el select2 de la receta aparece vacío.
        private IActionResult VistaConErrores(PlatoFormViewModel vm)
        {
            vm.Categorias = ObtenerCategoriasActivas();

            foreach (var item in vm.Receta.Where(r => r.IdProducto > 0))
            {
                var producto = _repositorioProducto.ObtenerPorId(item.IdProducto);
                item.NombreProducto = producto?.Nombre;
                item.UnidadMedida = producto?.Unidad_medida;
            }

            return View(vm);
        }

        private List<Categoria> ObtenerCategoriasActivas()
        {
            return _repositorioCategoria.ObtenerTodos().Where(c => c.Estado).ToList();
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
