using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioProducto
    {
        private readonly GastronomiaContext _context;

        public RepositorioProducto(GastronomiaContext context)
        {
            _context = context;
        }

        public int Alta(Producto p)
        {
            _context.Productos.Add(p);
            _context.SaveChanges();
            return p.IdProducto;
        }

        public int Baja(int id)
        {
            var producto = _context.Productos.Find(id);
            if (producto != null)
            {
                producto.Estado = false;
                return _context.SaveChanges();
            }
            return 0;
        }

        public int Modificar(Producto p)
        {
            _context.Productos.Update(p);
            return _context.SaveChanges();
        }

        public IList<Producto> Buscar(string q)
        {
            return _context.Productos
                .Where(p => p.Estado && p.Nombre != null && p.Nombre.Contains(q))
                .Take(20)
                .ToList();
        }

        public List<Producto> ObtenerLista(int pagNro, int tamPagina)
        {
            int offset = (pagNro - 1) * tamPagina;
            return _context.Productos
                .OrderBy(p => p.Nombre)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

        public int ObtenerCantidad()
        {
            return _context.Productos.Count(p => p.Estado);
        }

        public Producto? ObtenerPorId(int id)
        {
            return _context.Productos.Find(id);
        }
    }
}