using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioMesa
    {
        private readonly GastronomiaContext _context;

        public RepositorioMesa(GastronomiaContext context)
        {
            _context = context;
        }

        public int Alta(Mesa m)
        {
            m.Estado = false; // toda mesa nueva arranca sin pendientes de liberar
            m.Tipo = string.IsNullOrWhiteSpace(m.Tipo) ? "Mesa" : m.Tipo;

            _context.Mesas.Add(m);
            _context.SaveChanges();

            return m.IdMesa;
        }

        public int Baja(int id)
        {
            var mesa = _context.Mesas.Find(id);
            if (mesa == null)
                return 0;

            _context.Mesas.Remove(mesa);
            return _context.SaveChanges();
        }

        public int Modificar(Mesa m)
        {
            var existente = _context.Mesas.Find(m.IdMesa);
            if (existente == null)
                return 0;

            existente.Numero = m.Numero;
            existente.Capacidad = m.Capacidad;
            existente.Estado = m.Estado;
            existente.Tipo = string.IsNullOrWhiteSpace(m.Tipo) ? "Mesa" : m.Tipo;

            return _context.SaveChanges();
        }

        public Mesa? ObtenerPorId(int id)
        {
            return _context.Mesas
                .AsNoTracking()
                .FirstOrDefault(m => m.IdMesa == id);
        }

        public List<Mesa> ObtenerLista(int pagNro, int tamPagina)
        {
            int offset = (pagNro - 1) * tamPagina;

            return _context.Mesas
                .AsNoTracking()
                .OrderBy(m => m.Numero)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

        // Trae todas las mesas y puestos de barra, marcando:
        // - IdPedidoAbierto: el pedido Abierto de esa mesa (ocupada, se puede seguir cargando).
        // - IdPedidoPendienteLiberar: el último pedido Pagado de esa mesa, SOLO si
        //   Mesa.Estado == true (quedó pendiente de liberar tras cobrarse).
        // Se resuelve en memoria (no con un JOIN en SQL) a propósito: son tablas
        // chicas (14 mesas) y el cálculo mezcla una condición en C# (el flag de la
        // mesa) con la búsqueda del último pedido, algo incómodo de expresar en
        // una sola query LINQ-a-SQL sin perder legibilidad.
        public List<Mesa> ObtenerTodosConOcupacion()
        {
            var mesas = _context.Mesas.AsNoTracking().ToList();

            var pedidosAbiertos = _context.Pedidos
                .AsNoTracking()
                .Where(p => p.estado == Pedido.Estado.Abierto)
                .ToList();

            var pedidosPagados = _context.Pedidos
                .AsNoTracking()
                .Where(p => p.estado == Pedido.Estado.Pagado)
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            foreach (var mesa in mesas)
            {
                mesa.IdPedidoAbierto = pedidosAbiertos
                    .FirstOrDefault(p => p.IdMesa == mesa.IdMesa)?.IdPedido;

                mesa.IdPedidoPendienteLiberar = mesa.Estado
                    ? pedidosPagados.FirstOrDefault(p => p.IdMesa == mesa.IdMesa)?.IdPedido
                    : null;
            }

            return mesas;
        }

        // Libera una mesa que quedó pendiente (pedido ya pagado): apaga el flag
        // y la mesa vuelve a mostrarse libre en el Salón.
        public bool LiberarMesa(int idMesa)
        {
            var mesa = _context.Mesas.FirstOrDefault(m => m.IdMesa == idMesa);
            if (mesa == null) return false;

            mesa.Estado = false;
            _context.Mesas.Update(mesa);
            return _context.SaveChanges() > 0;
        }

        public IList<Mesa> Buscar(string q)
        {
            return _context.Mesas
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.Numero.ToString().Contains(q))
                .Take(20)
                .ToList();
        }
    }
}