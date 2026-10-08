using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioPlato
    {
        private readonly GastronomiaContext _context;

        public RepositorioPlato(GastronomiaContext context)
        {
            _context = context;
        }

        public int Alta(Plato p)
        {
            _context.Platos.Add(p);
            _context.SaveChanges();
            return p.IdPlato;
        }

        public int Baja(int id)
        {
            var plato = _context.Platos.Find(id);
            if (plato != null)
            {
                plato.Estado = false;
                return _context.SaveChanges();
            }
            return 0;
        }

        public int Modificar(Plato p)
        {
            var platoExistente = _context.Platos.Find(p.IdPlato);
            if (platoExistente != null)
            {
                platoExistente.Nombre = p.Nombre;
                platoExistente.PrecioVenta = p.PrecioVenta;
                platoExistente.IdCategoria = p.IdCategoria;
                return _context.SaveChanges();
            }
            return 0;
        }

        public IList<Plato> ObtenerTodos()
        {
            return _context.Platos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public IList<Plato> ObtenerPorCategoria(int idCategoria)
        {
            return _context.Platos
                .Include(p => p.Categoria)
                .Where(p => p.IdCategoria == idCategoria)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public Plato? ObtenerPorId(int id)
        {
            return _context.Platos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.IdPlato == id);
        }

        public IList<Plato> Buscar(string q)
        {
            return _context.Platos
                .Include(p => p.Categoria)
                .Where(p => p.Estado && p.Nombre != null && p.Nombre.Contains(q))
                .Take(20)
                .ToList();
        }

        public IList<Plato> ObtenerLista(int pagNro, int tamPagina)
        {
            int offset = (pagNro - 1) * tamPagina;
            return _context.Platos
                .Include(p => p.Categoria)
                .OrderByDescending(p => p.IdPlato)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

        public int ObtenerTotalRegistros()
        {
            return _context.Platos.Count();
        }

        public IList<Plato> BuscarPlato(String q)
        {
            return _context.Platos
                .Where(p => p.Estado && p.Nombre != null && p.Nombre.Contains(q))
                .Take(20)
                .ToList();
        }

    }
}