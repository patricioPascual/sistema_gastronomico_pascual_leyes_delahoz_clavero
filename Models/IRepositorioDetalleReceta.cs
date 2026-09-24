using System.Collections.Generic;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public interface IRepositorioDetalleReceta
    {
        IList<DetalleReceta> ObtenerPorPlato(int idPlato);
        int Alta(DetalleReceta d);
        int Modificar(DetalleReceta d);
        int Eliminar(int idDetalleReceta);

        int GuardarReceta(int idPlato, List<DetalleReceta> detalles);
    }
}