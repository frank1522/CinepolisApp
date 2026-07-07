using Microsoft.Data.SqlClient;
using System.Data;

namespace CinepolisApp.DAO
{
    public class VentaDAO
    {
        private readonly Conexion cn = new Conexion();

        public int RegistrarVenta(string cliente, string dni, decimal total, int idFuncion, int cantidadEntradas, string asientos, List<int> prodIds, List<int> prodCants, List<decimal> prodPrecios)
        {
            int idVentaGenerado = 0;

            using (SqlConnection oconexion = new SqlConnection(cn.GetCadenaSQL()))
            {
                oconexion.Open();
                // Usamos una transacción para asegurarnos de que se guarde todo o nada si hay error
                SqlTransaction transaccion = oconexion.BeginTransaction();

                try
                {
                    // 1. Insertar Cabecera de la Venta y recuperar el ID generado
                    string queryVenta = @"INSERT INTO Ventas (ClienteNombre, ClienteDni, Total) 
                                          VALUES (@cliente, @dni, @total);
                                          SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdVenta = new SqlCommand(queryVenta, oconexion, transaccion);
                    cmdVenta.Parameters.AddWithValue("@cliente", cliente);
                    cmdVenta.Parameters.AddWithValue("@dni", dni);
                    cmdVenta.Parameters.AddWithValue("@total", total);

                    idVentaGenerado = Convert.ToInt32(cmdVenta.ExecuteScalar());

                    // 2. Insertar Detalle de las Entradas/Asientos
                    string queryEntrada = @"INSERT INTO DetalleEntradas (IdVenta, IdFuncion, Cantidad, AsientosSeleccionados, Subtotal) 
                                            VALUES (@idVenta, @idFuncion, @cantidad, @asientos, @subtotal)";

                    SqlCommand cmdEntrada = new SqlCommand(queryEntrada, oconexion, transaccion);
                    cmdEntrada.Parameters.AddWithValue("@idVenta", idVentaGenerado);
                    cmdEntrada.Parameters.AddWithValue("@idFuncion", idFuncion);
                    cmdEntrada.Parameters.AddWithValue("@cantidad", cantidadEntradas);
                    cmdEntrada.Parameters.AddWithValue("@asientos", asientos);

                    // Supongamos que calculamos el subtotal de entradas en el controlador, aquí pasamos un valor temporal o el proporcional
                    cmdEntrada.Parameters.AddWithValue("@subtotal", total); // Para el trabajo simplificamos el flujo
                    cmdEntrada.ExecuteNonQuery();

                    // 3. Insertar Detalle de los Productos/Combos si es que eligió alguno
                    if (prodIds != null && prodIds.Count > 0)
                    {
                        string queryProd = @"INSERT INTO DetalleProductos (IdVenta, IdProducto, Cantidad, Subtotal) 
                                             VALUES (@idVenta, @idProducto, @cantidad, @subtotal)";

                        for (int i = 0; i < prodIds.Count; i++)
                        {
                            SqlCommand cmdProd = new SqlCommand(queryProd, oconexion, transaccion);
                            cmdProd.Parameters.AddWithValue("@idVenta", idVentaGenerado);
                            cmdProd.Parameters.AddWithValue("@idProducto", prodIds[i]);
                            cmdProd.Parameters.AddWithValue("@cantidad", prodCants[i]);
                            cmdProd.Parameters.AddWithValue("@subtotal", prodCants[i] * prodPrecios[i]);
                            cmdProd.ExecuteNonQuery();
                        }
                    }

                    transaccion.Commit(); // Si todo sale bien, guarda los cambios de golpe
                }
                catch (Exception)
                {
                    transaccion.Rollback(); // Si algo falla, limpia todo para no dejar basura
                    idVentaGenerado = 0;
                }
            }
            return idVentaGenerado;
        }
    }
}