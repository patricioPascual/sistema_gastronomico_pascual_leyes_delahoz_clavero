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
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            // El precio se toma del plato, no del formulario
            string sql = @"INSERT INTO detalle_pedido (id_pedido, id_plato, cantidad, precio_unitario, estado)
                           SELECT @idpedido, id_plato, @c, precio_venta, 'En Marcha'
                           FROM plato
                           WHERE id_plato = @idplato AND activo = 1;";

            int res;
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@idpedido", d.IdPedido);
                cmd.Parameters.AddWithValue("@idplato", d.IdPlato);
                cmd.Parameters.AddWithValue("@c", d.Cantidad);

                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException($"El plato {d.IdPlato} no existe o está dado de baja");
                res = (int)cmd.LastInsertedId;
            }

            RepositorioPedido.RecalcularTotal(d.IdPedido, conn, tx);
            tx.Commit();
            return res;
        }


        public bool Baja(int idDetallePedido)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            // Se busca antes del DELETE porque después el detalle ya no existe
            int idPedido = ObtenerIdPedido(idDetallePedido, conn, tx);

            // El enum de estado no tiene un valor de baja, así que el detalle se borra
            string sql = @"DELETE FROM detalle_pedido
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

        // Busca a qué pedido pertenece un detalle
        private int ObtenerIdPedido(int idDetallePedido, MySqlConnection conn, MySqlTransaction tx)
        {
            string sql = @"SELECT id_pedido FROM detalle_pedido
                           WHERE id_detalle_pedido = @id;";

            using var cmd = new MySqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", idDetallePedido);

            var resultado = cmd.ExecuteScalar();
            if (resultado == null)
                throw new InvalidOperationException("El detalle no existe");

            return Convert.ToInt32(resultado);
        }
    }

}