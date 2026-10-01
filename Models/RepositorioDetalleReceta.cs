using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioDetalleReceta
    {
        private readonly GastronomiaContext _context;

        public RepositorioDetalleReceta(GastronomiaContext context)
        {
            _context = context;
        }

        public IList<DetalleReceta> ObtenerPorPlato(int idPlato)
        {
            return _context.DetalleRecetas
                .Include(dr => dr.Producto)
                .Where(dr => dr.IdPlato == idPlato)
                .ToList();
        }

        public int Alta(DetalleReceta dr)
        {
            _context.DetalleRecetas.Add(dr);
            _context.SaveChanges();
            return dr.IdDetalleReceta;
        }

        public int Modificar(DetalleReceta dr)
        {
            var existente = _context.DetalleRecetas.Find(dr.IdDetalleReceta);
            if (existente != null)
            {
                existente.IdProducto = dr.IdProducto;
                existente.CantidadRequerida = dr.CantidadRequerida;
                return _context.SaveChanges();
            }
            return 0;
        }

        public int Eliminar(int idDetalleReceta)
        {
            var detalle = _context.DetalleRecetas.Find(idDetalleReceta);
            if (detalle != null)
            {
                _context.DetalleRecetas.Remove(detalle);
                return _context.SaveChanges();
            }
            return 0;
        }

        public int GuardarReceta(int idPlato, List<DetalleReceta> detalles)
        {
            var recetaAnterior = _context.DetalleRecetas.Where(dr => dr.IdPlato == idPlato).ToList();
            _context.DetalleRecetas.RemoveRange(recetaAnterior);

            if (detalles != null && detalles.Count > 0)
            {
                foreach (var dr in detalles)
                {
                    dr.IdPlato = idPlato;
                }
                _context.DetalleRecetas.AddRange(detalles);
            }

            return _context.SaveChanges();
        }
    }
}