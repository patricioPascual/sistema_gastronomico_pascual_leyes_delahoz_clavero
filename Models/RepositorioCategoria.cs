using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioCategoria
    {
        private readonly GastronomiaContext _context;

        public RepositorioCategoria(GastronomiaContext context)
        {
            _context = context;
        }

        public IList<Categoria> ObtenerTodos()
        {
            return _context.Categorias
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        public Categoria? ObtenerPorId(int id)
        {
            return _context.Categorias.Find(id);
        }
    }
}