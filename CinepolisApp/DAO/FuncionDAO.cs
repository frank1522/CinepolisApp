using CinepolisApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;


namespace CinepolisApp.DAO
{
    public class FuncionDAO
    {
        private readonly Conexion cn = new Conexion();

        public List<FuncionDTO> ListarPorPelicula(int idPelicula)
        {
            List<FuncionDTO> lista = new 
                List<FuncionDTO>();

            using (SqlConnection oconexion = new SqlConnection(cn.GetCadenaSQL()))
            {
                string query = @"SELECT f.IdFuncion, f.IdPelicula, c.Nombre as NombreCine, 
                                f.Fecha, f.Hora, f.PrecioEntrada, f.Sala 
                                FROM Funciones f
                                INNER JOIN Cines c ON f.IdCine = c.IdCine
                                WHERE f.IdPelicula = @idPelicula";

                SqlCommand cmd = new SqlCommand(query, oconexion);
                cmd.Parameters.AddWithValue("@idPelicula", idPelicula);
                cmd.CommandType= CommandType.Text;

                oconexion.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new FuncionDTO
                        {
                            IdFuncion = Convert.ToInt32(dr["IdFuncion"]),
                            IdPelicula = Convert.ToInt32(dr["IdPelicula"]),
                            NombreCine = dr["NombreCine"].ToString(),
                            Fecha = Convert.ToDateTime(dr["Fecha"]),
                            Hora = dr["Hora"].ToString(),
                            PrecioEntrada = Convert.ToDecimal(dr["PrecioEntrada"]),
                            Sala = dr["Sala"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }

    // Un objeto temporal para transportar los datos unidos con el nombre del cine
    public class FuncionDTO
    {
        public int IdFuncion { get; set; }
        public int IdPelicula { get; set; }
        public string NombreCine { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public decimal PrecioEntrada { get; set; }
        public string Sala { get; set; }
    }
}