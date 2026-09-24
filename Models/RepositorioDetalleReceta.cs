using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioDetalleReceta : RepositorioBase, IRepositorioDetalleReceta
    {
        public RepositorioDetalleReceta(IConfiguration configuration) : base(configuration)
        {
        }

        public IList<DetalleReceta> ObtenerPorPlato(int idPlato)
        {
            var lista = new List<DetalleReceta>();
            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT dr.id_detalle_receta, dr.id_plato, dr.id_producto, dr.cantidad_requerida,
                                        pr.nombre AS producto_nombre, pr.unidad_medida, pr.cantidad_stock, pr.estado AS producto_estado
                                 FROM detalle_receta dr
                                 INNER JOIN producto pr ON pr.id_producto = dr.id_producto
                                 WHERE dr.id_plato = @idPlato;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlato", idPlato);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new DetalleReceta
                            {
                                IdDetalleReceta = reader.GetInt32("id_detalle_receta"),
                                IdPlato = reader.GetInt32("id_plato"),
                                IdProducto = reader.GetInt32("id_producto"),
                                CantidadRequerida = reader.GetDecimal("cantidad_requerida"),
                                Producto = new Producto
                                {
                                    IdProducto = reader.GetInt32("id_producto"),
                                    Nombre = reader.GetString("producto_nombre"),
                                    Unidad_medida = reader.GetString("unidad_medida"),
                                    Cantidad_stock = reader.GetDecimal("cantidad_stock"),
                                    Estado = reader.GetBoolean("producto_estado")
                                }
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public int Alta(DetalleReceta d)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"INSERT INTO detalle_receta (id_plato, id_producto, cantidad_requerida)
                                 VALUES (@idPlato, @idProducto, @cantidadRequerida);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPlato", d.IdPlato);
                    cmd.Parameters.AddWithValue("@idProducto", d.IdProducto);
                    cmd.Parameters.AddWithValue("@cantidadRequerida", d.CantidadRequerida);

                    conn.Open();
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return res;
        }

        public int Modificar(DetalleReceta d)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"UPDATE detalle_receta
                                 SET id_producto = @idProducto,
                                     cantidad_requerida = @cantidadRequerida
                                 WHERE id_detalle_receta = @idDetalleReceta;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idProducto", d.IdProducto);
                    cmd.Parameters.AddWithValue("@cantidadRequerida", d.CantidadRequerida);
                    cmd.Parameters.AddWithValue("@idDetalleReceta", d.IdDetalleReceta);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Eliminar(int idDetalleReceta)
        {
            int res = -1;
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = "DELETE FROM detalle_receta WHERE id_detalle_receta = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idDetalleReceta);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int GuardarReceta(int idPlato, List<DetalleReceta> detalles)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlBorrar = "DELETE FROM detalle_receta WHERE id_plato = @idPlato";
                        using (var cmdBorrar = new MySqlCommand(sqlBorrar, conn, tx))
                        {
                            cmdBorrar.Parameters.AddWithValue("@idPlato", idPlato);
                            cmdBorrar.ExecuteNonQuery();
                        }

                        string sqlInsertar = @"INSERT INTO detalle_receta (id_plato, id_producto, cantidad_requerida)
                                               VALUES (@idPlato, @idProducto, @cantidadRequerida);";

                        foreach (var d in detalles)
                        {
                            using (var cmdInsertar = new MySqlCommand(sqlInsertar, conn, tx))
                            {
                                cmdInsertar.Parameters.AddWithValue("@idPlato", idPlato);
                                cmdInsertar.Parameters.AddWithValue("@idProducto", d.IdProducto);
                                cmdInsertar.Parameters.AddWithValue("@cantidadRequerida", d.CantidadRequerida);
                                cmdInsertar.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return detalles.Count;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}