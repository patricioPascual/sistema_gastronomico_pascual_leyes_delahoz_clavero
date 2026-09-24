using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioPlato : RepositorioBase, IRepositorioPlato
    {
        public RepositorioPlato(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Plato p)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO plato (nombre, precio_venta, activo, id_categoria)
                                 VALUES (@nombre, @precioVenta, @activo, @idCategoria);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                    cmd.Parameters.AddWithValue("@precioVenta", p.PrecioVenta);
                    cmd.Parameters.AddWithValue("@activo", p.Estado);
                    cmd.Parameters.AddWithValue("@idCategoria", p.IdCategoria);

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
                string sql = "UPDATE plato SET activo = @es WHERE id_plato = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@es", false);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Modificar(Plato p)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE plato
                                 SET nombre = @nombre,
                                     precio_venta = @precioVenta,
                                     id_categoria = @idCategoria
                                 WHERE id_plato = @idPlato;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                    cmd.Parameters.AddWithValue("@precioVenta", p.PrecioVenta);
                    cmd.Parameters.AddWithValue("@idCategoria", p.IdCategoria);
                    cmd.Parameters.AddWithValue("@idPlato", p.IdPlato);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public IList<Plato> ObtenerTodos()
        {
            var lista = new List<Plato>();
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_plato, nombre, precio_venta, activo, id_categoria
                                 FROM plato
                                 ORDER BY nombre;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearPlato(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public IList<Plato> ObtenerPorCategoria(int idCategoria)
        {
            var lista = new List<Plato>();
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_plato, nombre, precio_venta, activo, id_categoria
                                 FROM plato
                                 WHERE id_categoria = @idCategoria
                                 ORDER BY nombre;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearPlato(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public Plato ObtenerPorId(int id)
        {
            Plato p = null;
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_plato, nombre, precio_venta, activo, id_categoria
                                 FROM plato
                                 WHERE id_plato = @id;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p = MapearPlato(reader);
                        }
                    }
                }
            }
            return p;
        }

        public IList<Plato> Buscar(string q)
        {
            var lista = new List<Plato>();
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_plato, nombre, precio_venta, activo, id_categoria
                                 FROM plato
                                 WHERE activo = 1 AND nombre LIKE @q
                                 LIMIT 20;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + q + "%");
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearPlato(reader));
                        }
                    }
                }
            }
            return lista;
        }

        private Plato MapearPlato(MySqlDataReader reader)
        {
            return new Plato
            {
                IdPlato = reader.GetInt32("id_plato"),
                Nombre = reader.GetString("nombre"),
                PrecioVenta = reader.GetDecimal("precio_venta"),
                Estado = reader.GetBoolean("activo"),
                IdCategoria = reader.GetInt32("id_categoria")
            };
        }
    }
}