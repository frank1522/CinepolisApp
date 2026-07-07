using CinepolisApp.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
namespace CinepolisApp.DAO
{
    public class PeliculaDAO
    {
        private readonly Conexion cn = new Conexion();
        public List<Pelicula> ListarTodas()
        {
            List<Pelicula> lista = new List<Pelicula>();

            using (SqlConnection oconexion = new SqlConnection(cn.GetCadenaSQL()))
            {
               
                string query = "SELECT IdPelicula, Titulo, Genero, Clasificacion, Duracion, ImagenUrl FROM Peliculas";
                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.CommandType = CommandType.Text;

                oconexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pelicula
                        {
                            IdPelicula = Convert.ToInt32(dr["IdPelicula"]),
                            Titulo = dr["Titulo"].ToString(),
                            Genero = dr["Genero"].ToString(),
                            Clasificacion = dr["Clasificacion"].ToString(),
                            Duracion = Convert.ToInt32(dr["Duracion"]),
                            ImagenUrl = dr["ImagenUrl"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}