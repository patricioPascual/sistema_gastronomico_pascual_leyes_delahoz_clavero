using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers
{
    public class PlatoController : Controller
    {
        private readonly GastronomiaContext _context;

        // Inyectamos directamente el contexto de EF Core ESTA PARTE VA EN PROGRAMS y tambien tenemos que crear un .cs con la configuracion como con sequelize
        public PlatoController(GastronomiaContext context)
        {
            _context = context;
        }

        // Listado con Paginación y Categoria incluida (JOIN)
        public IActionResult Index(int pagina = 1)
        {
            int tamPagina = 10;
            int totalRegistros = _context.Platos.Count();

            // Paginacion en base de datos con LINQ
            var platos = _context.Platos
                .Include(p => p.Categoria) // Reemplaza al JOIN manual
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);

            return View(platos);
        }

        // Vista de Creación (GET)
        public IActionResult Crear()
        {
            var vm = new PlatoFormViewModel
            {
                Categorias = _context.Categorias.Where(c => c.Estado).ToList()
            };
            return View(vm);
        }

        // Guardar Plato y su Receta (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _context.Categorias.Where(c => c.Estado).ToList();
                return View(vm);
            }

            vm.Plato.Estado = true;

            // 1. Agregamos el plato al contexto
            _context.Platos.Add(vm.Plato);
            _context.SaveChanges(); // Al guardar, EF Core asigna el ID autoincremental a vm.Plato.IdPlato

            // 2. Procesamos y guardamos los detalles de la receta
            if (vm.Receta != null)
            {
                var detalles = vm.Receta
                    .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                    .Select(r => new DetalleReceta
                    {
                        IdPlato = vm.Plato.IdPlato,
                        IdProducto = r.IdProducto,
                        CantidadRequerida = r.CantidadRequerida
                    })
                    .ToList();

                _context.DetalleRecetas.AddRange(detalles);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        // Baja lógica del Plato
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var plato = _context.Platos.Find(id);
            if (plato != null)
            {
                plato.Estado = false; // Baja lógica
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // Vista de Edición (GET)
        public IActionResult Editar(int id)
        {
            // Buscamos el plato incluyendo su categoría
            var plato = _context.Platos
                .FirstOrDefault(p => p.IdPlato == id);

            if (plato == null)
            {
                return NotFound();
            }

            // Obtenemos los detalles de la receta actual del plato junto con sus productos asociados
            var receta = _context.DetalleRecetas
                .Include(d => d.Producto)
                .Where(d => d.IdPlato == id)
                .ToList();

            // Armamos el ViewModel de la misma forma que en el repositorio original
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
                Categorias = _context.Categorias.Where(c => c.Estado).ToList()
            };

            return View(vm);
        }

        // Guardar Edición de Plato y su Receta (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, PlatoFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categorias = _context.Categorias.Where(c => c.Estado).ToList();
                return View(vm);
            }

            var platoExistente = _context.Platos.Find(id);
            if (platoExistente == null)
            {
                return NotFound();
            }

            // 1. Actualizamos las propiedades del plato
            platoExistente.Nombre = vm.Plato.Nombre;
            platoExistente.PrecioVenta = vm.Plato.PrecioVenta;
            platoExistente.IdCategoria = vm.Plato.IdCategoria;

            _context.Platos.Update(platoExistente);

            // 2. Actualizamos la receta: Una estrategia limpia es eliminar los detalles anteriores y agregar los nuevos del formulario
            var recetaAnterior = _context.DetalleRecetas.Where(d => d.IdPlato == id).ToList();
            _context.DetalleRecetas.RemoveRange(recetaAnterior);

            if (vm.Receta != null)
            {
                var nuevosDetalles = vm.Receta
                    .Where(r => r.IdProducto > 0 && r.CantidadRequerida > 0)
                    .Select(r => new DetalleReceta
                    {
                        IdPlato = id,
                        IdProducto = r.IdProducto,
                        CantidadRequerida = r.CantidadRequerida
                    })
                    .ToList();

                _context.DetalleRecetas.AddRange(nuevosDetalles);
            }

            // Guardamos todos los cambios juntos en la base de datos de manera atómica
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}