using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioRol
    {
        private readonly GastronomiaContext _context;

        public RepositorioRol(GastronomiaContext context)
        {
            _context = context;
        }

        public IList<Rol> ObtenerTodos()
        {
            return _context.Roles.OrderBy(r => r.Nombre).ToList();
        }

        public Rol? ObtenerPorId(int id)
        {
            return _context.Roles.Find(id);
        }
    }
}