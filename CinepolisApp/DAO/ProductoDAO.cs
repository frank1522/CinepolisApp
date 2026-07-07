using CinepolisApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CinepolisApp.DAO
{
    public class ProductoDAO
    {
        private readonly Conexion cn = new Conexion();

        public List<Producto> ListarTodos()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection oconexion = new SqlConnection(cn.GetCadenaSQL()))
            {
                string query = "SELECT IdProducto, Nombre, Descripcion, Precio, ImagenUrl FROM Productos";
                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.CommandType = CommandType.Text;

                oconexion.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Producto
                        {
                            IdProducto = Convert.ToInt32(dr["IdProducto"]),
                            Nombre = dr["Nombre"].ToString(),
                            Descripcion = dr["Descripcion"].ToString(),
                            Precio = Convert.ToDecimal(dr["Precio"]),
                            ImagenUrl = dr["ImagenUrl"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}