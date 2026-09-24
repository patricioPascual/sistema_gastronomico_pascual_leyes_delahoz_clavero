using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioDetallePedido : RepositorioBase
    {

        public RepositorioDetallePedido(IConfiguration configuration) : base(configuration)
        {
        }


        public int Alta(DetallePedido d)
        {
            int res = -1;
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO detalle_pedido (id_pedido ,id_plato,cantidad,precio_unitario,estado) 
                              VALUES (@idpedido,@idplato,@c,@precio,@es); 
                              SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idpedido", d.IdPedido);
                    cmd.Parameters.AddWithValue("@idplato", d.IdPlato);
                    cmd.Parameters.AddWithValue("@c", d.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@es", d.estado);

                    conn.Open();
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return res;
        }


        public bool Baja(int idDetallePedido)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            // Se busca antes del UPDATE porque después el detalle ya no está activo
            int idPedido = ObtenerIdPedido(idDetallePedido, conn, tx);

            string sql = @"UPDATE detalle_pedido SET estado = 0
                   WHERE id_detalle_pedido = @id;";
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", idDetallePedido);
                cmd.ExecuteNonQuery();
            }

            RepositorioPedido.RecalcularTotal(idPedido, conn, tx);
            tx.Commit();
            return true;
        }


        public bool ModificarCantidad(int idDetallePedido, int nuevaCantidad)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            int idPedido = ObtenerIdPedido(idDetallePedido, conn, tx);

            string sql = @"UPDATE detalle_pedido SET cantidad = @cantidad
                   WHERE id_detalle_pedido = @id;";
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@cantidad", nuevaCantidad);
                cmd.Parameters.AddWithValue("@id", idDetallePedido);
                cmd.ExecuteNonQuery();
            }

            RepositorioPedido.RecalcularTotal(idPedido, conn, tx);
            tx.Commit();
            return true;
        }

        // Busca a qué pedido pertenece un detalle activo
        private int ObtenerIdPedido(int idDetallePedido, MySqlConnection conn, MySqlTransaction tx)
        {
            string sql = @"SELECT id_pedido FROM detalle_pedido
                           WHERE id_detalle_pedido = @id AND estado = 1;";

            using var cmd = new MySqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", idDetallePedido);

            var resultado = cmd.ExecuteScalar();
            if (resultado == null)
                throw new InvalidOperationException("El detalle no existe o ya fue dado de baja");

            return Convert.ToInt32(resultado);
        }
    }

}