using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioPedido : RepositorioBase
    {

        public RepositorioPedido(IConfiguration configuration) : base(configuration)
        {
        }


        public int Alta(Pedido p)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            string sqlPedido = @"INSERT INTO pedido (fecha_hora, id_mesa, id_empleado, total, estado)
                         VALUES (@fecha, @idMesa, @idEmpleado, 0, 'Abierto');
                         SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(sqlPedido, conn, tx))
            {
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@idMesa", p.IdMesa);
                cmd.Parameters.AddWithValue("@idEmpleado", p.IdEmpleado);
                p.IdPedido = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string sqlDetalle = @"INSERT INTO detalle_pedido (id_pedido, id_plato, cantidad, precio_unitario, estado)
                          SELECT @idPedido, id_plato, @cantidad, precio_venta, 'En Marcha'
                          FROM plato
                          WHERE id_plato = @idPlato AND activo = 1;";

            foreach (var d in p.Detalles)
            {
                using var cmd = new MySqlCommand(sqlDetalle, conn, tx);
                cmd.Parameters.AddWithValue("@idPedido", p.IdPedido);
                cmd.Parameters.AddWithValue("@idPlato", d.IdPlato);
                cmd.Parameters.AddWithValue("@cantidad", d.Cantidad);

                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException($"El plato {d.IdPlato} no existe o está dado de baja");
            }

            RecalcularTotal(p.IdPedido, conn, tx);

            tx.Commit();
            return p.IdPedido;
        }


        public bool Baja(int idpedido)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE pedido
                       SET estado = 'Cancelado'
                       WHERE id_pedido = @id AND estado = 'Abierto';";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idpedido);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }


        public bool ModificarPedido(Pedido p)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE pedido 
                       SET id_mesa = @idMesa, id_empleado = @idEmpleado 
                       WHERE id_pedido = @id AND estado = 'Abierto';";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idMesa", p.IdMesa);
                    cmd.Parameters.AddWithValue("@idEmpleado", p.IdEmpleado);
                    cmd.Parameters.AddWithValue("@id", p.IdPedido);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public static void RecalcularTotal(int idPedido, MySqlConnection conn,MySqlTransaction tx)
        {
            string sql = @"UPDATE pedido
                   SET total = (SELECT COALESCE(SUM(cantidad * precio_unitario), 0)
                                FROM detalle_pedido
                                WHERE id_pedido = @id)
                   WHERE id_pedido = @id;";

            using var cmd = new MySqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", idPedido);
            cmd.ExecuteNonQuery();
        }


        public List<Pedido> ObtenerLista(int pagNro, int tamPagina)
        {
            var lista = new List<Pedido>();
            int offset = (pagNro - 1) * tamPagina;

            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT p.id_pedido, p.fecha_hora, p.estado, p.total, p.id_mesa, p.id_empleado,
                                      m.numero AS numero_mesa,
                                      e.nombre AS nombre_empleado, e.apellido AS apellido_empleado
                               FROM pedido p
                               INNER JOIN mesa m ON p.id_mesa = m.id_mesa
                               INNER JOIN empleado e ON p.id_empleado = e.id_empleado
                               ORDER BY p.fecha_hora DESC
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
                            lista.Add(LeerPedido(reader));
                        }
                    }
                }
            }
            return lista;
        }


        public int ObtenerCantidad()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM pedido;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }


        public Pedido? ObtenerPorId(int idPedido)
        {
            Pedido? pedido = null;

            string sqlCabecera = @"SELECT p.id_pedido, p.fecha_hora, p.estado, p.total, p.id_mesa, p.id_empleado,
                                          m.numero AS numero_mesa,
                                          e.nombre AS nombre_empleado, e.apellido AS apellido_empleado
                                   FROM pedido p
                                   INNER JOIN mesa m ON p.id_mesa = m.id_mesa
                                   INNER JOIN empleado e ON p.id_empleado = e.id_empleado
                                   WHERE p.id_pedido = @id;";

            string sqlDetalles = @"SELECT d.id_detalle_pedido, d.id_pedido, d.id_plato, d.cantidad, d.precio_unitario, d.estado,
                                          pl.nombre AS nombre_plato
                                   FROM detalle_pedido d
                                   INNER JOIN plato pl ON d.id_plato = pl.id_plato
                                   WHERE d.id_pedido = @id;";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sqlCabecera, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idPedido);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            pedido = LeerPedido(reader);
                    }
                }

                if (pedido != null)
                {
                    using (var cmd = new MySqlCommand(sqlDetalles, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idPedido);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pedido.Detalles.Add(new DetallePedido
                                {
                                    IdDetallePedido = reader.GetInt32("id_detalle_pedido"),
                                    IdPedido = reader.GetInt32("id_pedido"),
                                    IdPlato = reader.GetInt32("id_plato"),
                                    Cantidad = reader.GetInt32("cantidad"),
                                    PrecioUnitario = reader.GetDecimal("precio_unitario"),
                                    // En la base el enum es 'En Marcha', en C# es EnMarcha
                                    estado = reader.IsDBNull(reader.GetOrdinal("estado"))
                                        ? DetallePedido.Estado.EnMarcha
                                        : Enum.Parse<DetallePedido.Estado>(reader.GetString("estado").Replace(" ", "")),
                                    Plato = new Plato
                                    {
                                        IdPlato = reader.GetInt32("id_plato"),
                                        Nombre = reader.GetString("nombre_plato")
                                    }
                                });
                            }
                        }
                    }
                }
            }

            return pedido;
        }


        private static Pedido LeerPedido(MySqlDataReader reader)
        {
            return new Pedido
            {
                IdPedido = reader.GetInt32("id_pedido"),
                FechaHora = reader.GetDateTime("fecha_hora"),
                estado = reader.IsDBNull(reader.GetOrdinal("estado"))
                    ? Pedido.Estado.Abierto
                    : Enum.Parse<Pedido.Estado>(reader.GetString("estado")),
                Total = reader.GetDecimal("total"),
                IdMesa = reader.GetInt32("id_mesa"),
                Mesa = new Mesa
                {
                    IdMesa = reader.GetInt32("id_mesa"),
                    Numero = reader.GetInt32("numero_mesa")
                },
                IdEmpleado = reader.GetInt32("id_empleado"),
                Empleado = new Empleado
                {
                    IdEmpleado = reader.GetInt32("id_empleado"),
                    Nombre = reader.GetString("nombre_empleado"),
                    Apellido = reader.GetString("apellido_empleado")
                }
            };
        }

    }

}