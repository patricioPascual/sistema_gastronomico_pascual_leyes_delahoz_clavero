using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioProveedor :RepositorioBase
    {
     

       public RepositorioProveedor(IConfiguration configuration) : base(configuration)
        {
        }

    
        public int Alta(Proveedor proveedor)
        {
            int idCreado = 0;
            string query = @"INSERT INTO proveedor (nombre, cuit, telefono, direccion, estado)
                             VALUES (@nombre, @cuit, @telefono, @direccion, 1);";

            using (var conn = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", proveedor.Nombre);
                    cmd.Parameters.AddWithValue("@cuit", (object?)proveedor.Cuit ?? DBNull.Value );
                    cmd.Parameters.AddWithValue("@telefono", (object?)proveedor.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", (object?)proveedor.Direccion  ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    idCreado = Convert.ToInt32(cmd.LastInsertedId);
                }
            }

            return idCreado;
        }

   
        public bool Modificar(Proveedor proveedor)
        {
            int filasAfectadas = 0;
            string query = @"UPDATE proveedor 
                             SET nombre = @nombre,
                                 cuit = @cuit,
                                 telefono = @telefono,
                                 direccion = @direccion
                             WHERE id_proveedor = @id_proveedor AND estado = 1;";

            using (var conn = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_proveedor", proveedor.IdProveedor);
                    cmd.Parameters.AddWithValue("@nombre", proveedor.Nombre);
                    cmd.Parameters.AddWithValue("@cuit",(object?)proveedor.Cuit ?? DBNull.Value );
                    cmd.Parameters.AddWithValue("@telefono", (object?)proveedor.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", (object?)proveedor.Direccion ?? DBNull.Value);

                    conn.Open();
                    filasAfectadas = cmd.ExecuteNonQuery();
                }
            }

            return filasAfectadas > 0;
        }

        public bool Baja(int idProveedor)
        {
            int filasAfectadas = 0;
            string query = @"UPDATE proveedor 
                             SET estado = 0 
                             WHERE id_proveedor = @id_proveedor AND estado = 1;";

            using (var conn = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_proveedor", idProveedor);

                    conn.Open();
                    filasAfectadas = cmd.ExecuteNonQuery();
                }
            }

            return filasAfectadas > 0;
        }

        public Proveedor? ObtenerPorId(int idProveedor)
        {
            Proveedor? proveedor = null;
            string query = @"SELECT id_proveedor, nombre, cuit, telefono,direccion, estado
                             FROM proveedor
                             WHERE id_proveedor = @id_proveedor AND estado = 1;";

            using (var conn = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_proveedor", idProveedor);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            proveedor = new Proveedor
                            {
                                IdProveedor = reader.GetInt32("id_proveedor"),
                                Nombre = reader.GetString("nombre"),
                                Cuit = reader.IsDBNull(reader.GetOrdinal("cuit")) ? null : reader.GetString("cuit"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                                Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? null : reader.GetString("direccion"),
                                Estado = reader.GetBoolean("estado")
                            };
                        }
                    }
                }
            }

            return proveedor;
        }

     
        public List<Proveedor> ObtenerTodos()
        {
            var lista = new List<Proveedor>();
            string query = @"SELECT id_proveedor, nombre, cuit, telefono, direccion, estado
                             FROM proveedor
                             WHERE estado = 1
                             ORDER BY nombre ASC;";

            using (var conn = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Proveedor
                            {
                                IdProveedor = reader.GetInt32("id_proveedor"),
                                Nombre = reader.GetString("nombre"),
                                Cuit = reader.IsDBNull(reader.GetOrdinal("cuit")) ? null : reader.GetString("cuit"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null : reader.GetString("telefono"),
                                Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? null : reader.GetString("direccion"),
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