using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioUsuario
    {
        private readonly GastronomiaContext _context;

        public RepositorioUsuario(GastronomiaContext context)
        {
            _context = context;
        }

        public IList<Usuario> ObtenerTodos()
        {
            return _context.Usuarios
                .Include(u => u.Empleado)
                .Include(u => u.Rol)
                .ToList();
        }

        public Usuario? ObtenerPorId(int id)
        {
            return _context.Usuarios
                .Include(u => u.Empleado)
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.IdUsuario == id);
        }

        public Usuario? ObtenerPorUsername(string username)
        {
            return _context.Usuarios
                .Include(u => u.Empleado)
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Username == username && u.Estado);
        }

        public int Alta(Usuario u)
        {
            u.Estado = true;
            _context.Usuarios.Add(u);
            _context.SaveChanges();
            return u.IdUsuario;
        }

        public int Modificar(Usuario u)
        {
            _context.Usuarios.Update(u);
            return _context.SaveChanges();
        }

        public int Baja(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Estado = false;
                return _context.SaveChanges();
            }
            return 0;
        }
    }
}