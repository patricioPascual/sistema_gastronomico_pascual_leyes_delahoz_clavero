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

public IList<Producto> Buscar(string q)
{
    var lista = new List<Producto>();
    using (var conn = new MySqlConnection(connectionString))
    {
        string query = @"SELECT id_producto, nombre, cantidad_stock, unidad_medida 
                         FROM producto 
                         WHERE estado = 1 AND nombre LIKE @q 
                         LIMIT 20;";

        using (var cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@q", "%" + q + "%");
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Producto
                    {
                        IdProducto = reader.GetInt32("id_producto"),
                        Nombre = reader.GetString("nombre"),
                        Cantidad_stock = reader.GetDecimal("cantidad_stock"),
                        Unidad_medida = reader.GetString("unidad_medida")
                    });
                }
            }
        }
    }
    return lista;
}

    public IList<Producto> ObtenerLista(int pagNro =1, int tamPagina=10)
        {   
             IList<Producto> res = new List<Producto>();
            int offset = (pagNro -1)* tamPagina;
            using (var conn = new MySqlConnection(connectionString))
            {
                String sql=@" SELECT IdProducto,Nombre,  Cantidad_stock ,Unidad_medida,Precio_costo,Estado 
                   FROM Producto 
                   WHERE Estado=1 
                   ORDER BY IdProducto
                   LIMIT @tamPagina OFFSET @offset ";
                   using (var cmd = new MySqlCommand(sql,conn))
                {
                    cmd.Parameters.AddWithValue("@tamPagina", tamPagina);
                    cmd.Parameters.AddWithValue("@offset", offset);
                    conn.Open();

                    using (var reader=cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Producto p = new Producto
                            {   
                        IdProducto=Convert.ToInt32(reader[nameof(Producto.IdProducto)]),
                        Nombre=reader[nameof(Producto.Nombre)]?.ToString()??"",
                        Cantidad_stock=reader.GetDecimal(nameof(Producto.Cantidad_stock)),
                        Unidad_medida=reader.GetString(nameof(Producto.Unidad_medida)),
                        Precio_costo=reader.GetDecimal(nameof(Producto.Precio_costo)),
                        Estado=Convert.ToBoolean(reader[nameof(Producto.Estado)]),

                            };
                            res.Add(p);
                        
                        }
                    }
                }
            }
              return res;
        }
     public int ObtenerCantidad()
        {
            int total = 0;
            using (var conn = new MySqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Producto WHERE estado = 1;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    conn.Open();
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return total;
        }

    }
}