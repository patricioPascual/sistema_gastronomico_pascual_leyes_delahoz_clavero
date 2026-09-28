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
                string query = @"INSERT INTO mesa (numero, capacidad, estado, tipo)
                                 VALUES (@numero, @capacidad, @estado, @tipo);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@numero", m.Numero);
                    cmd.Parameters.AddWithValue("@capacidad", m.Capacidad);
                    cmd.Parameters.AddWithValue("@estado", false); // toda mesa nueva arranca Libre
                    cmd.Parameters.AddWithValue("@tipo", string.IsNullOrWhiteSpace(m.Tipo) ? "Mesa" : m.Tipo);

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
                                     estado = @estado,
                                     tipo = @tipo
                                 WHERE id_mesa = @idMesa;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@numero", m.Numero);
                    cmd.Parameters.AddWithValue("@capacidad", m.Capacidad);
                    cmd.Parameters.AddWithValue("@estado", m.Estado);
                    cmd.Parameters.AddWithValue("@tipo", string.IsNullOrWhiteSpace(m.Tipo) ? "Mesa" : m.Tipo);
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
                string sql = @"SELECT id_mesa, numero, capacidad, estado, tipo
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
                                Estado = reader.GetBoolean("estado"),
                                Tipo = reader.GetString("tipo")
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
                string sql = @"SELECT id_mesa, numero, capacidad, estado, tipo
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
                                Estado = reader.GetBoolean("estado"),
                                Tipo = reader.GetString("tipo")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // Trae todas las mesas y puestos de barra, marcando con qué pedido abierto
        // está ocupada cada una (null si está libre). Es la fuente de verdad para
        // el Salón: no depende de que Mesa.Estado esté sincronizado.
        public List<Mesa> ObtenerTodosConOcupacion()
        {
            var lista = new List<Mesa>();

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT m.id_mesa, m.numero, m.capacidad, m.estado, m.tipo,
                                      p.id_pedido AS id_pedido_abierto
                              FROM mesa m
                              LEFT JOIN pedido p ON p.id_mesa = m.id_mesa AND p.estado = 'Abierto';";

                using (var cmd = new MySqlCommand(sql, conn))
                {
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
                                Estado = reader.GetBoolean("estado"),
                                Tipo = reader.GetString("tipo"),
                                IdPedidoAbierto = reader.IsDBNull(reader.GetOrdinal("id_pedido_abierto"))
                                    ? null
                                    : reader.GetInt32("id_pedido_abierto")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public IList<Mesa> Buscar(string q)
        {
            var lista = new List<Mesa>();

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_mesa, numero, capacidad, estado, tipo
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
                                Estado = reader.GetBoolean("estado"),
                                Tipo = reader.GetString("tipo")
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}