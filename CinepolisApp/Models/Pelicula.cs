namespace CinepolisApp.Models
{
    public class Pelicula
    {
        public int IdPelicula { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public string Clasificacion { get; set; }
        public int Duracion { get; set; }
        public string ImagenUrl { get; set; }
    }
}