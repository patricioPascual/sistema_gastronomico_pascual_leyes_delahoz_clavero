namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    // Lista cerrada de unidades de medida de los insumos.
    // La usan la validación del modelo Producto y los <select> de Alta/Modificar,
    // así el servidor y la vista no pueden quedar desalineados.
    public static class UnidadesMedida
    {
        public static readonly IReadOnlyDictionary<string, string> Todas = new Dictionary<string, string>
        {
            { "kg", "Kilogramos (kg)" },
            { "gr", "Gramos (gr)" },
            { "lt", "Litros (lt)" },
            { "ml", "Mililitros (ml)" },
            { "unidad", "Unidades (un)" },
            { "docena", "Docena" },
            { "caja", "Caja" },
            { "lata", "Lata / Envase" }
        };

        public static bool EsValida(string? unidad)
        {
            return !string.IsNullOrWhiteSpace(unidad) && Todas.ContainsKey(unidad);
        }
    }
}
