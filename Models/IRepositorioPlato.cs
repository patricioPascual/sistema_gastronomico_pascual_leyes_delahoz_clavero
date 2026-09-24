using System.Collections.Generic;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public interface IRepositorioPlato
    {
        IList<Plato> ObtenerTodos();
        IList<Plato> ObtenerPorCategoria(int idCategoria);
        Plato ObtenerPorId(int id);
        IList<Plato> Buscar(string q);
        int Alta(Plato p);
        int Modificar(Plato p);
        int Baja(int id);
    }
}