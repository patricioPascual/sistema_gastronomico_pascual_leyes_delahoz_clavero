using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioEmpleado : RepositorioBase
    {
        public RepositorioEmpleado(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Empleado e)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO empleado (legajo, nombre, apellido, dni, telefono, fecha_ingreso, activo)
                                 VALUES (@legajo, @nombre, @apellido, @dni, @telefono, @fechaIngreso, @activo);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@legajo", e.Legajo);
                    cmd.Parameters.AddWithValue("@nombre", e.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", e.Apellido);
                    cmd.Parameters.AddWithValue("@dni", e.Dni);
                    cmd.Parameters.AddWithValue("@telefono", (object?)e.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fechaIngreso", e.Fecha_ingreso);
                    cmd.Parameters.AddWithValue("@activo", true);

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
                string sql = "UPDATE empleado SET activo = @activo WHERE id_empleado = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@activo", false);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificar(Empleado e)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE empleado
                                 SET legajo = @legajo,
                                     nombre = @nombre,
                                     apellido = @apellido,
                                     dni = @dni,
                                     telefono = @telefono,
                                     fecha_ingreso = @fechaIngreso,
                                     activo = @activo
                                 WHERE id_empleado = @idEmpleado;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@legajo", e.Legajo);
                    cmd.Parameters.AddWithValue("@nombre", e.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", e.Apellido);
                    cmd.Parameters.AddWithValue("@dni", e.Dni);
                    cmd.Parameters.AddWithValue("@telefono", (object?)e.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fechaIngreso", e.Fecha_ingreso);
                    cmd.Parameters.AddWithValue("@activo", e.Activo);
                    cmd.Parameters.AddWithValue("@idEmpleado", e.IdEmpleado);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public Empleado? ObtenerPorId(int id)
        {
            Empleado? e = null;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id_empleado, legajo, nombre, apellido, dni, telefono, fecha_ingreso, activo
                              FROM empleado
                              WHERE id_empleado = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            e = new Empleado
                            {
                                IdEmpleado = reader.GetInt32("id_empleado"),
                                Legajo = reader.GetString("legajo"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Dni = reader.GetString("dni"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                                Fecha_ingreso = reader.GetDateTime("fecha_ingreso"),
                                Activo = reader.GetBoolean("activo")
                            };
                        }
                    }
                }
            }

            return e;
        }

        public List<Empleado> ObtenerLista(int pagNro, int tamPagina)
        {
            var lista = new List<Empleado>();
            int offset = (pagNro - 1) * tamPagina;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id_empleado, legajo, nombre, apellido, dni, telefono, fecha_ingreso, activo
                              FROM empleado
                              ORDER BY apellido, nombre
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
                            lista.Add(new Empleado
                            {
                                IdEmpleado = reader.GetInt32("id_empleado"),
                                Legajo = reader.GetString("legajo"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Dni = reader.GetString("dni"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                                Fecha_ingreso = reader.GetDateTime("fecha_ingreso"),
                                Activo = reader.GetBoolean("activo")
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
                string sql = "SELECT COUNT(*) FROM empleado WHERE activo = 1;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return total;
        }

        public IList<Empleado> Buscar(string q)
        {
            var lista = new List<Empleado>();

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT id_empleado, legajo, nombre, apellido, dni
                                 FROM empleado
                                 WHERE activo = 1 AND (nombre LIKE @q OR apellido LIKE @q OR legajo LIKE @q)
                                 LIMIT 20;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + q + "%");
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Empleado
                            {
                                IdEmpleado = reader.GetInt32("id_empleado"),
                                Legajo = reader.GetString("legajo"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Dni = reader.GetString("dni")
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}