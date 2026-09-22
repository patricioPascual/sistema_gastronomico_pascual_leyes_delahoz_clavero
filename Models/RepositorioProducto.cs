using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;

namespace  sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioProducto : RepositorioBase
    {
       
        public RepositorioProducto(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Producto p)
        {
            int res = -1;

            using (var conn = new MySqlConnection(connectionString)) 
            {
                string query = @"INSERT INTO producto (nombre, cantidad_stock, unidad_medida, precio_costo, estado) 
                                 VALUES (@nombre, @cantidadStock, @unidadMedida, @precioCosto, @estado);
                                 SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                    cmd.Parameters.AddWithValue("@cantidadStock", p.Cantidad_stock);
                    cmd.Parameters.AddWithValue("@unidadMedida", p.Unidad_medida);
                    cmd.Parameters.AddWithValue("@precioCosto", p.Precio_costo);
                    cmd.Parameters.AddWithValue("@estado", p.Estado);

                    conn.Open();
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return res;
        }



        public int Baja (int id)
        {
            int res=-1; 
            using (var conn= new MySqlConnection(connectionString))
            {
                string sql= "UPDATE Producto SET estado = @es WHERE IdProducto = @id";
                using (var cmd= new MySqlCommand(sql,conn))
                {
                    cmd.Parameters.AddWithValue("@id",id);
                    cmd.Parameters.AddWithValue("@es",false);

                    conn.Open();
                    res = cmd.ExecuteNonQuery();
                }
            }
             return res;
        }

        public int Modificar(Producto p)
{
    int res = -1;

    using (var conn = new MySqlConnection(connectionString))
    {
        string query = @"UPDATE producto 
                         SET nombre = @nombre, 
                             cantidad_stock = @cantidadStock, 
                             unidad_medida = @unidadMedida, 
                             precio_costo = @precioCosto, 
                             estado = @estado
                         WHERE id_producto = @idProducto;";

        using (var cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@cantidadStock", p.Cantidad_stock);
            cmd.Parameters.AddWithValue("@unidadMedida", p.Unidad_medida);
            cmd.Parameters.AddWithValue("@precioCosto", p.Precio_costo);
            cmd.Parameters.AddWithValue("@estado", p.Estado);
            cmd.Parameters.AddWithValue("@idProducto", p.IdProducto);

            conn.Open();
            res = cmd.ExecuteNonQuery(); 
        }
    }

    return res;
}
    }
}