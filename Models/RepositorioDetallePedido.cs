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
       

      public int Alta (DetallePedido d)
        {
            int res=-1;
            using (var conn= new MySqlConnection(connectionString))
            {
                String sql= @"INSERT INTO Detalle_pedido (id_pedido ,id_plato,cantidad,precio_unitario,estado) 
                              VALUES (@idpedido,@idplato,@c,@precio,@es); 
                              SELECT LAST_INSERT_ID();";
                    
                using (var cmd = new MySqlCommand(sql,conn))
                {
                    cmd.Parameters.AddWithValue("@idpedido",d.IdPedido);
                    cmd.Parameters.AddWithValue("@idplato",d.IdPlato);
                    cmd.Parameters.AddWithValue("@c",d.Cantidad);
                    cmd.Parameters.AddWithValue("@precio",d.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@es",d.Estado);

                     conn.Open();
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return res;
        } 
       

       public bool Baja(int idDetallePedido)
{
    using (var conn = new MySqlConnection(connectionString))
    {
        string sql = @"UPDATE detalle_pedido 
                       SET estado = 0 
                       WHERE id_detalle_pedido = @id;";

        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", idDetallePedido);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}


      public bool ModificarCantidad(int idDetallePedido, int nuevaCantidad)
{
    using (var conn = new MySqlConnection(connectionString))
    {
        string sql = @"UPDATE detalle_pedido 
                       SET cantidad = @cantidad 
                       WHERE id_detalle_pedido = @id;";

        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@cantidad", nuevaCantidad);
            cmd.Parameters.AddWithValue("@id", idDetallePedido);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
    } 

}