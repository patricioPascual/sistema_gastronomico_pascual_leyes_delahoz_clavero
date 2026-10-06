using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioEmpleado
    {
        private readonly GastronomiaContext _context;

        public RepositorioEmpleado(GastronomiaContext context)
        {
            _context = context;
        }

        public int Alta(Empleado e)
        {
            e.Activo = true;
            _context.Empleados.Add(e);
            _context.SaveChanges();
            return e.IdEmpleado;
        }

        public int Baja(int id)
        {
            var empleado = _context.Empleados.Find(id);
            if (empleado != null)
            {
                empleado.Activo = false;
                return _context.SaveChanges();
            }
            return 0;
        }

        public int Modificar(Empleado e)
        {
            _context.Empleados.Update(e);
            return _context.SaveChanges();
        }

        public Empleado? ObtenerPorId(int id)
        {
            return _context.Empleados.Find(id);
        }

        public List<Empleado> ObtenerLista(int pagNro, int tamPagina)
        {
            int offset = (pagNro - 1) * tamPagina;
            return _context.Empleados
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

        public int ObtenerCantidad()
        {
            return _context.Empleados.Count(e => e.Activo);
        }

        public IList<Empleado> Buscar(string q)
        {
            return _context.Empleados
                .Where(e => e.Activo && (e.Nombre != null && e.Nombre.Contains(q) ||
                                         e.Apellido != null && e.Apellido.Contains(q) ||
                                         e.Legajo != null && e.Legajo.Contains(q)))
                .Take(20)
                .ToList();
        }
    }
}