using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioCompra : RepositorioBase
    {
         public RepositorioCompra(IConfiguration configuration) : base(configuration)
        {
        }

    public int Alta(Compra compra)
{
    int idCompraCreada = 0;

    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                // se inserta en compra
                string queryCompra = @"INSERT INTO compra (fecha_hora, id_proveedor, numero_comprobante, total_compra, id_empleado, estado) 
                                       VALUES (@fecha_hora, @id_proveedor, @numero_comprobante, @total_compra, @id_empleado, 1);";

                using (var cmd = new MySqlCommand(queryCompra, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@fecha_hora", compra.FechaHora == default ? DateTime.Now : compra.FechaHora);
                    cmd.Parameters.AddWithValue("@id_proveedor", compra.IdProveedor);
                    cmd.Parameters.AddWithValue("@numero_comprobante", (object?)compra.NumeroComprobante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@total_compra", compra.TotalCompra);
                    cmd.Parameters.AddWithValue("@id_empleado", compra.IdEmpleado);

                    cmd.ExecuteNonQuery();
                    
                    // Obtener el ID generado por AUTO_INCREMENT
                    idCompraCreada = Convert.ToInt32(cmd.LastInsertedId);
                }

                // se inserta en el detalle con el idCompraCreada 
                string queryDetalle = @"INSERT INTO detalle_compra (id_compra, id_producto, cantidad_ingresada, precio_costo_unitario) 
                                        VALUES (@id_compra, @id_producto, @cantidad_ingresada, @precio_costo_unitario);";

                string queryUpdateStock = @"UPDATE producto 
                                            SET cantidad_stock = cantidad_stock + @cantidad_ingresada,
                                                precio_costo = @precio_costo_unitario 
                                            WHERE id_producto = @id_producto;";

                foreach (var detalle in compra.Detalles)
                {
                    
                    using (var cmdDetalle = new MySqlCommand(queryDetalle, connection, transaction))
                    {
                        cmdDetalle.Parameters.AddWithValue("@id_compra", idCompraCreada);
                        cmdDetalle.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                        cmdDetalle.Parameters.AddWithValue("@cantidad_ingresada", detalle.CantidadIngresada);
                        cmdDetalle.Parameters.AddWithValue("@precio_costo_unitario", detalle.PrecioCostoUnitario);
                        cmdDetalle.ExecuteNonQuery();
                    }
                  //Sumo al stock y pongo precio reciente
                    using (var cmdStock = new MySqlCommand(queryUpdateStock, connection, transaction))
                    {
                        cmdStock.Parameters.AddWithValue("@cantidad_ingresada", detalle.CantidadIngresada);
                        cmdStock.Parameters.AddWithValue("@precio_costo_unitario", detalle.PrecioCostoUnitario);
                        cmdStock.Parameters.AddWithValue("@id_producto", detalle.IdProducto);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    return idCompraCreada;
}      public bool Baja(int idCompra)
{
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                //  Obtener los insumos de esta compra para descontar el stock ingresado
                string sqlDetalles = "SELECT id_producto, cantidad_ingresada FROM detalle_compra WHERE id_compra = @id_compra;";
                var detalles = new List<(int IdProducto, decimal Cantidad)>();

                using (var cmdDetalles = new MySqlCommand(sqlDetalles, connection, transaction))
                {
                    cmdDetalles.Parameters.AddWithValue("@id_compra", idCompra);
                    using (var reader = cmdDetalles.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add((reader.GetInt32("id_producto"), reader.GetDecimal("cantidad_ingresada")));
                        }
                    }
                }

                //  Descontar el stock en la tabla producto
                string sqlStock = "UPDATE producto SET cantidad_stock = cantidad_stock - @cantidad WHERE id_producto = @id_producto;";
                foreach (var item in detalles)
                {
                    using (var cmdStock = new MySqlCommand(sqlStock, connection, transaction))
                    {
                        cmdStock.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdStock.Parameters.AddWithValue("@id_producto", item.IdProducto);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                //  Dar de baja lógica a la compra
                string query = "UPDATE compra SET estado = 0 WHERE id_compra = @id_compra;";
                using (var cmd = new MySqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@id_compra", idCompra);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                }

                transaction.Rollback();
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

        public bool Modificar(Compra compra)
        {
            int filasAfectadas = 0;
            string query = @"UPDATE compra 
                             SET  id_proveedor = @proveedor, 
                                 numero_comprobante = @numero_comprobante, 
                                 total_compra = @total_compra, 
                                 id_empleado = @id_empleado
                             WHERE id_compra = @id_compra AND estado = 1;";

            using (var connection = new MySqlConnection(connectionString))
            {
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id_compra", compra.IdCompra);
                    cmd.Parameters.AddWithValue("@proveedor", compra.IdProveedor);
                    cmd.Parameters.AddWithValue("@numero_comprobante", (object?)compra.NumeroComprobante ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@total_compra", compra.TotalCompra);
                    cmd.Parameters.AddWithValue("@id_empleado", compra.IdEmpleado);

                    connection.Open();
                    filasAfectadas = cmd.ExecuteNonQuery();
                }
            }

            return filasAfectadas > 0;
        }

       public Compra? ObtenerPorId(int idCompra)
{
    Compra? compra = null;

    string queryCabecera = @"SELECT c.id_compra, c.fecha_hora, c.id_proveedor, c.numero_comprobante, c.total_compra, c.id_empleado,c.estado,
                                    p.nombre AS nombre_proveedor,
                                    e.nombre AS nombre_empleado, e.apellido AS apellido_empleado
                             FROM compra c
                             INNER JOIN proveedor p ON c.id_proveedor = p.id_proveedor
                             INNER JOIN empleado e ON c.id_empleado = e.id_empleado
                             WHERE c.id_compra = @id_compra AND c.estado = 1;";

    string queryDetalles = @"SELECT dc.id_detalle_compra, dc.id_compra, dc.id_producto, dc.cantidad_ingresada, dc.precio_costo_unitario,
                                    p.nombre AS nombre_producto, p.unidad_medida
                             FROM detalle_compra dc
                             INNER JOIN producto p ON dc.id_producto = p.id_producto
                             WHERE dc.id_compra = @id_compra;";

    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        // COMPRA
        using (var cmd = new MySqlCommand(queryCabecera, connection))
        {
            cmd.Parameters.AddWithValue("@id_compra", idCompra);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    compra = new Compra
                    {
                        IdCompra = reader.GetInt32("id_compra"),
                        FechaHora = reader.GetDateTime("fecha_hora"),
                        IdProveedor = reader.GetInt32("id_proveedor"),
                        Estado=reader.GetBoolean("estado"),
                        Proveedor = new Proveedor
                        {
                            IdProveedor = reader.GetInt32("id_proveedor"),
                            Nombre = reader.GetString("nombre_proveedor")
                        },
                        NumeroComprobante = reader.IsDBNull(reader.GetOrdinal("numero_comprobante")) ? null : reader.GetString("numero_comprobante"),
                        TotalCompra = reader.GetDecimal("total_compra"),
                        IdEmpleado = reader.GetInt32("id_empleado"),
                        Empleado = new Empleado
                        {
                            IdEmpleado = reader.GetInt32("id_empleado"),
                            Nombre = reader.GetString("nombre_empleado"),
                            Apellido = reader.GetString("apellido_empleado")
                        },
                        Detalles = new List<DetalleCompra>()
                    };
                }
            }
        }

        // DETALLES 
        if (compra != null)
        {
            using (var cmdDet = new MySqlCommand(queryDetalles, connection))
            {
                cmdDet.Parameters.AddWithValue("@id_compra", idCompra);
                using (var reader = cmdDet.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        compra.Detalles.Add(new DetalleCompra
                        {
                            IdDetalleCompra = reader.GetInt32("id_detalle_compra"),
                            IdCompra = reader.GetInt32("id_compra"),
                            IdProducto = reader.GetInt32("id_producto"),
                            CantidadIngresada = reader.GetDecimal("cantidad_ingresada"),
                            PrecioCostoUnitario = reader.GetDecimal("precio_costo_unitario"),
                            Producto = new Producto
                            {
                                IdProducto = reader.GetInt32("id_producto"),
                                Nombre = reader.GetString("nombre_producto"),
                                Unidad_medida = reader.GetString("unidad_medida")
                            }
                        });
                    }
                }
            }
        }
    }

    return compra;
}
public List<Compra> ObtenerLista(int pagNro, int tamPagina)
{
    var lista = new List<Compra>();
    int offset = (pagNro - 1) * tamPagina;

    using (var conn = new MySqlConnection(connectionString))
    {
        string sql = @"SELECT c.id_compra, c.fecha_hora, c.id_proveedor, c.numero_comprobante, 
                              c.total_compra, c.id_empleado, c.estado,
                              p.nombre AS nombre_proveedor,
                              e.nombre AS nombre_empleado, e.apellido AS apellido_empleado
                       FROM compra c
                       INNER JOIN proveedor p ON c.id_proveedor = p.id_proveedor
                       INNER JOIN empleado e ON c.id_empleado = e.id_empleado
                       WHERE c.estado = 1
                       ORDER BY c.fecha_hora DESC
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
                    var c = new Compra
                    {
                        IdCompra = Convert.ToInt32(reader["id_compra"]),
                        FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                        IdProveedor = Convert.ToInt32(reader["id_proveedor"]),
                        Estado = Convert.ToBoolean(reader["estado"]),
                        Proveedor = new Proveedor
                        {
                            IdProveedor = Convert.ToInt32(reader["id_proveedor"]),
                            Nombre = reader["nombre_proveedor"].ToString()!
                        },
                        NumeroComprobante = reader["numero_comprobante"] != DBNull.Value ? reader["numero_comprobante"].ToString() : null,
                        TotalCompra = Convert.ToDecimal(reader["total_compra"]),
                        IdEmpleado = Convert.ToInt32(reader["id_empleado"]),
                        Empleado = new Empleado
                        {
                            IdEmpleado = Convert.ToInt32(reader["id_empleado"]),
                            Nombre = reader["nombre_empleado"].ToString()!,
                            Apellido = reader["apellido_empleado"].ToString()!
                        }
                    };
                    lista.Add(c);
                }
            }
        }
    }
    return lista;
}
    }
}
    

