using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioMesa : RepositorioBase
    {
        public RepositorioMesa(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Mesa m)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO mesa (numero, capacidad, estado)
                                 VALUES (@numero, @capacidad, @estado);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@numero", m.Numero);
                    cmd.Parameters.AddWithValue("@capacidad", m.Capacidad);
                    cmd.Parameters.AddWithValue("@estado", false); // toda mesa nueva arranca Libre

                    conn.Open();
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = "DELETE FROM mesa WHERE id_mesa = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificar(Mesa m)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE mesa
                                 SET numero = @numero,
                                     capacidad = @capacidad,
                                     estado = @estado
                                 WHERE id_mesa = @idMesa;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@numero", m.Numero);
                    cmd.Parameters.AddWithValue("@capacidad", m.Capacidad);
                    cmd.Parameters.AddWithValue("@estado", m.Estado);
                    cmd.Parameters.AddWithValue("@idMesa", m.IdMesa);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public Mesa? ObtenerPorId(int id)
        {
            Mesa? m = null;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id_mesa, numero, capacidad, estado
                              FROM mesa
                              WHERE id_mesa = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            m = new Mesa
                            {
                                IdMesa = reader.GetInt32("id_mesa"),
                                Numero = reader.GetInt32("numero"),
                                Capacidad = reader.GetInt32("capacidad"),
                                Estado = reader.GetBoolean("estado")
                            };
                        }
                    }
                }
            }

            return m;
        }

        public List<Mesa> ObtenerLista(int pagNro, int tamPagina)
        {
            var lista = new List<Mesa>();
            int offset = (pagNro - 1) * tamPagina;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id_mesa, numero, capacidad, estado
                            FROM mesa
                            ORDER BY numero
                            LIMIT @tamPagina OFFSET @offset;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@tamPagina", tamPagina);
                    cmd.Parameters.AddWithValue("@offset", offset);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Mesa
                            {
                                IdMesa = reader.GetInt32("id_mesa"),
                                Numero = reader.GetInt32("numero"),
                                Capacidad = reader.GetInt32("capacidad"),
                                Estado = reader.GetBoolean("estado")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public int ObtenerCantidad()
        {
            int total = 0;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM mesa;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return total;
        }

        public IList<Mesa> Buscar(string q)
        {
            var lista = new List<Mesa>();

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_mesa, numero, capacidad, estado
                                FROM mesa
                                WHERE numero LIKE @q
                                LIMIT 20;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + q + "%");
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Mesa
                            {
                                IdMesa = reader.GetInt32("id_mesa"),
                                Numero = reader.GetInt32("numero"),
                                Capacidad = reader.GetInt32("capacidad"),
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