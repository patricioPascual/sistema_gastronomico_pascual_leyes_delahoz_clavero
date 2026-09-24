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
                         VALUES (@fecha, @idMesa, @idEmpleado, 0, 1);
                         SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(sqlPedido, conn, tx))
            {
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@idMesa", p.IdMesa);
                cmd.Parameters.AddWithValue("@idEmpleado", p.IdEmpleado);
                p.IdPedido = Convert.ToInt32(cmd.ExecuteScalar());
            }

            string sqlDetalle = @"INSERT INTO detalle_pedido (id_pedido, id_plato, cantidad, precio_unitario, estado)
                          SELECT @idPedido, id_plato, @cantidad, precio_venta, 1
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
                       SET estado = 0 
                       WHERE id_pedido = @id;";

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
                       WHERE id_pedido = @id AND estado = 1;";

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
                                WHERE id_pedido = @id AND estado = 1)
                   WHERE id_pedido = @id;";

            using var cmd = new MySqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", idPedido);
            cmd.ExecuteNonQuery();
        }

    }

}