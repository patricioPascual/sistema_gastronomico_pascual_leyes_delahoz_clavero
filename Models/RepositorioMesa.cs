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
            m.Estado = false; // toda mesa nueva arranca Libre
            m.Tipo = string.IsNullOrWhiteSpace(m.Tipo) ? "Mesa" : m.Tipo;

            _context.Mesas.Add(m);
            _context.SaveChanges();

            // EF completa el id autogenerado en el propio objeto tras el INSERT
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

        // Trae todas las mesas y puestos de barra, marcando con qué pedido abierto
        // está ocupada cada una (null si está libre). Es la fuente de verdad para
        // el Salón: no depende de que Mesa.Estado esté sincronizado.
        //
        // Equivalente LINQ del LEFT JOIN ... ON ... AND p.estado='Abierto' que tenía
        // la versión en SQL crudo: el filtro por 'Abierto' se aplica ANTES del join,
        // sobre la secuencia de pedidos (GroupJoin), nunca en un Where() posterior
        // al resultado ya unido — es la misma distinción ON-vs-WHERE de siempre.
        public List<Mesa> ObtenerTodosConOcupacion()
        {
            var pedidosAbiertos = _context.Pedidos
                .Where(p => p.estado == Pedido.Estado.Abierto);

            return _context.Mesas
                .AsNoTracking()
                .GroupJoin(
                    pedidosAbiertos,
                    mesa => mesa.IdMesa,
                    pedido => pedido.IdMesa,
                    (mesa, pedidos) => new { mesa, pedido = pedidos.FirstOrDefault() })
                .Select(x => new Mesa
                {
                    IdMesa = x.mesa.IdMesa,
                    Numero = x.mesa.Numero,
                    Capacidad = x.mesa.Capacidad,
                    Estado = x.mesa.Estado,
                    Tipo = x.mesa.Tipo,
                    IdPedidoAbierto = x.pedido != null ? x.pedido.IdPedido : (int?)null
                })
                .ToList();
        }

        public IList<Mesa> Buscar(string q)
        {
            // AsEnumerable() antes del filtro: ToString() sobre un int no se puede
            // traducir a SQL en LINQ-to-Entities. Como mesa es una tabla chica y fija
            // (14 filas), traer todo a memoria y filtrar en C# no tiene costo real.
            return _context.Mesas
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.Numero.ToString().Contains(q))
                .Take(20)
                .ToList();
        }
    }
}