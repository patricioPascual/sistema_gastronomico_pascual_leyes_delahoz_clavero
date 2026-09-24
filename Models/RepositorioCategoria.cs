using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioCategoria : RepositorioBase
    {
        public RepositorioCategoria(IConfiguration configuration) : base(configuration)
        {
        }

        public IList<Categoria> ObtenerTodos()
        {
            var lista = new List<Categoria>();
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT id_categoria, nombre, estado FROM categoria WHERE estado = 1 ORDER BY nombre;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Categoria
                            {
                                IdCategoria = reader.GetInt32("id_categoria"),
                                Nombre = reader.GetString("nombre"),
                                Estado = reader.GetBoolean("estado")
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
