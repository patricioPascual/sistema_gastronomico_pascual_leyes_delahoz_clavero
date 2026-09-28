using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioInforme : RepositorioBase
    {
        public RepositorioInforme(IConfiguration configuration) : base(configuration)
        {
        }

     public IList<PlatoMasVendidoDto> ObtenerPlatosMasVendidos(DateTime desde, DateTime hasta)
    {
        var lista = new List<PlatoMasVendidoDto>();
        using (var conn = new MySqlConnection(connectionString))
        {
            string query = @"SELECT p.nombre, SUM(dp.cantidad) AS total_cant, SUM(dp.subtotal) AS total_recaudado
                             FROM detalle_pedido dp
                             INNER JOIN pedido pe ON dp.id_pedido = pe.id_pedido
                             INNER JOIN plato p ON dp.id_plato = p.id_plato
                             WHERE pe.fecha_hora BETWEEN @desde AND @hasta AND pe.estado = 1
                             GROUP BY p.id_plato, p.nombre
                             ORDER BY total_cant DESC
                             LIMIT 10;";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new PlatoMasVendidoDto
                        {
                            NombrePlato = reader.GetString("nombre"),
                            CantidadVendida = reader.GetInt32("total_cant"),
                            TotalRecaudado = reader.GetDecimal("total_recaudado")
                        });
                    }
                }
            }
        }
        return lista;
    }



    public IList<ComprasPorProveedorDto> ObtenerComprasPorProveedor(DateTime desde, DateTime hasta)
        {
            var lista = new List<ComprasPorProveedorDto>();

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT pr.nombre AS nombre_proveedor, 
                                        COUNT(c.id_compra) AS cantidad_compras, 
                                        SUM(c.total_compra) AS total_acumulado
                                 FROM compra c
                                 INNER JOIN proveedor pr ON c.id_proveedor = pr.id_proveedor
                                 WHERE c.estado = 1 
                                   AND c.fecha_hora >= @desde 
                                   AND c.fecha_hora <= @hasta
                                 GROUP BY pr.id_proveedor, pr.nombre
                                 ORDER BY total_acumulado DESC;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ComprasPorProveedorDto
                            {
                                NombreProveedor = reader.GetString("nombre_proveedor"),
                                CantidadCompras = reader.GetInt32("cantidad_compras"),
                                TotalAcumulado = reader.GetDecimal("total_acumulado")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public BalanceGeneralDto ObtenerBalanceGeneral(DateTime desde, DateTime hasta)
        {
            var balance = new BalanceGeneralDto();

            using (var conn = new MySqlConnection(connectionString))
            {
                string query = @"SELECT 
                                    COALESCE((SELECT SUM(pe.total) 
                                              FROM pedido pe 
                                              WHERE pe.estado = 1 
                                                AND pe.fecha_hora >= @desde 
                                                AND pe.fecha_hora <= @hasta), 0) AS total_ventas,
                                    
                                    COALESCE((SELECT SUM(c.total_compra) 
                                              FROM compra c 
                                              WHERE c.estado = 1 
                                                AND c.fecha_hora >= @desde 
                                                AND c.fecha_hora <= @hasta), 0) AS total_compras;";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            balance.TotalVentas = reader.GetDecimal("total_ventas");
                            balance.TotalCompras = reader.GetDecimal("total_compras");
                        }
                    }
                }
            }

            return balance;
        }


        public IList<VentasPorMozoDto> ObtenerVentasPorMozo(DateTime desde, DateTime hasta)
{
    var lista = new List<VentasPorMozoDto>();

    using (var conn = new MySqlConnection(connectionString))
    {
        string query = @"SELECT CONCAT(e.apellido, ', ', e.nombre) AS nombre_empleado,
                                COUNT(p.id_pedido) AS cantidad_pedidos,
                                SUM(p.total) AS total_ventas
                         FROM pedido p
                         INNER JOIN empleado e ON p.id_empleado = e.id_empleado
                         WHERE p.estado = 1 
                           AND p.fecha_hora >= @desde 
                           AND p.fecha_hora <= @hasta
                         GROUP BY e.id_empleado, e.apellido, e.nombre
                         ORDER BY total_ventas DESC;";

        using (var cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@desde", desde);
            cmd.Parameters.AddWithValue("@hasta", hasta);

            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new VentasPorMozoDto
                    {
                        NombreEmpleado = reader.GetString("nombre_empleado"),
                        CantidadPedidos = reader.GetInt32("cantidad_pedidos"),
                        TotalVentas = reader.GetDecimal("total_ventas")
                    });
                }
            }
        }
    }

    return lista;
}
    }
}

     

    
