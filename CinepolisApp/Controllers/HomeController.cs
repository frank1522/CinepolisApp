using CinepolisApp.DAO;
using CinepolisApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinepolisApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly PeliculaDAO _peliculaDAO = new PeliculaDAO();

        public IActionResult Index()
        {
            // Jalamos las películas desde la base de datos
            List<Pelicula> listaPeliculas = _peliculaDAO.ListarTodas();

            // Se las mandamos a la vista Index.cshtml
            return View(listaPeliculas);
        }
    }
}