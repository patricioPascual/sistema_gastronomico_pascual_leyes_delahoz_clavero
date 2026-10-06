using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class PlatoFormViewModel
    {
        public Plato Plato { get; set; } = new Plato();
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
        public List<DetalleRecetaFormItem> Receta { get; set; } = new List<DetalleRecetaFormItem>();
        public IFormFile? ArchivoImagen { get; set; }
        public bool EliminarImagen { get; set; }
    }


    public class DetalleRecetaFormItem
    {
        public int IdDetalleReceta { get; set; }
        public int IdProducto { get; set; }
        public decimal CantidadRequerida { get; set; }
        public string? UnidadMedida { get; set; }
        public string? NombreProducto { get; set; }
    }
}

