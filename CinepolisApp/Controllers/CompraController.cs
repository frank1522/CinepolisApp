using CinepolisApp.DAO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace CinepolisApp.Controllers
{
    public class CompraController : Controller
    {
        private readonly FuncionDAO _funcionDAO = new FuncionDAO();
        private readonly ProductoDAO _productoDAO = new ProductoDAO();
        private readonly VentaDAO _ventaDAO = new VentaDAO();

        // 1. Mostrar funciones disponibles para la película seleccionada
        public IActionResult SeleccionarFuncion(int id)
        {
            var funciones = _funcionDAO.ListarPorPelicula(id);
            if (funciones.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(funciones);
        }

        // 2. Mostrar el mapa de asientos (Butacas)
        public IActionResult SeleccionarAsientos(int idFuncion)
        {
            ViewBag.IdFuncion = idFuncion;
            return View();
        }

        // POST: Capturar los asientos seleccionados y guardarlos en Sesión
        [HttpPost]
        public IActionResult GuardarAsientos(int idFuncion, string asientos)
        {
            if (string.IsNullOrEmpty(asientos))
            {
                TempData["Error"] = "Debes seleccionar al menos un asiento.";
                return RedirectToAction("SeleccionarAsientos", new { idFuncion = idFuncion });
            }

            HttpContext.Session.SetInt32("Carrito_IdFuncion", idFuncion);
            HttpContext.Session.SetString("Carrito_Asientos", asientos);

            int cantidadEntradas = asientos.Split(',').Length;
            HttpContext.Session.SetInt32("Carrito_CantidadEntradas", cantidadEntradas);
            return RedirectToAction("SeleccionarCombos", "Compra");
        }

        // 3. GET: Mostrar la dulcería
        public IActionResult SeleccionarCombos()
        {
            var productos = _productoDAO.ListarTodos();
            return View(productos);
        }

        // POST: Recibir los combos elegidos y guardarlos en la Sesión
        [HttpPost]
        public IActionResult GuardarCombos(List<int> idProducto, List<int> cantidad)
        {
            List<string> listaIds = new List<string>();
            List<string> listaCants = new List<string>();

            for (int i = 0; i < idProducto.Count; i++)
            {
                if (cantidad[i] > 0)
                {
                    listaIds.Add(idProducto[i].ToString());
                    listaCants.Add(cantidad[i].ToString());
                }
            }

            HttpContext.Session.SetString("Carrito_ProductosIds", string.Join(",", listaIds));
            HttpContext.Session.SetString("Carrito_ProductosCantidades", string.Join(",", listaCants));

            return RedirectToAction("ResumenVenta", "Compra");
        }

        // 4. GET: Mostrar Resumen del Carrito de Compras (Tu Boleta + Pasarela)
        [HttpGet]
        public IActionResult ResumenVenta()
        {
            int? idFuncion = HttpContext.Session.GetInt32("Carrito_IdFuncion");
            string asientos = HttpContext.Session.GetString("Carrito_Asientos");
            int? cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas");

            if (idFuncion == null || string.IsNullOrEmpty(asientos))
            {
                return RedirectToAction("Index", "Home");
            }

            decimal precioEntrada = 15.00m;
            decimal totalEntradas = (cantEntradas ?? 0) * precioEntrada;
            decimal totalCombos = 0;

            List<ComboElegidoDTO> combosLista = new List<ComboElegidoDTO>();

            string prodIdsStr = HttpContext.Session.GetString("Carrito_ProductosIds");
            string prodCantsStr = HttpContext.Session.GetString("Carrito_ProductosCantidades");

            if (!string.IsNullOrEmpty(prodIdsStr) && !string.IsNullOrEmpty(prodCantsStr))
            {
                var ids = prodIdsStr.Split(',').Select(int.Parse).ToList();
                var cants = prodCantsStr.Split(',').Select(int.Parse).ToList();
                var todosProductos = _productoDAO.ListarTodos();

                for (int i = 0; i < ids.Count; i++)
                {
                    var prod = todosProductos.FirstOrDefault(p => p.IdProducto == ids[i]);
                    if (prod != null)
                    {
                        decimal subtotalItem = prod.Precio * cants[i];
                        totalCombos += subtotalItem;

                        combosLista.Add(new ComboElegidoDTO
                        {
                            Nombre = prod.Nombre,
                            Cantidad = cants[i],
                            PrecioUnitario = prod.Precio,
                            Subtotal = subtotalItem
                        });
                    }
                }
            }

            // Datos que van directo a la vista del formulario de pago
            ViewBag.IdFuncion = idFuncion;
            ViewBag.Asientos = asientos;
            ViewBag.SubtotalEntradas = totalEntradas.ToString("F2");
            ViewBag.SubtotalSnacks = totalCombos.ToString("F2");
            ViewBag.TotalPagar = (totalEntradas + totalCombos).ToString("F2");
            ViewBag.ListaCombos = combosLista;

            return View(); // Tu vista ResumenVenta renderizará el formulario de pago
        }

        // Objeto temporal para estructurar los combos en el resumen
        public class ComboElegidoDTO
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
        }

        // 5. POST: Procesar el Pago Final, Registrar en BD y mandar a la pantalla del QR
        [HttpPost]
        public IActionResult TerminarCompra(string clienteNombre, string clienteDni)
        {
            int idFuncion = HttpContext.Session.GetInt32("Carrito_IdFuncion") ?? 0;
            string asientos = HttpContext.Session.GetString("Carrito_Asientos");
            int cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas") ?? 0;

            decimal precioEntrada = 15.00m;
            decimal total = cantEntradas * precioEntrada;

            string prodIdsStr = HttpContext.Session.GetString("Carrito_ProductosIds");
            string prodCantsStr = HttpContext.Session.GetString("Carrito_ProductosCantidades");

            List<int> listaIds = new List<int>();
            List<int> listaCants = new List<int>();
            List<decimal> listaPrecios = new List<decimal>();

            if (!string.IsNullOrEmpty(prodIdsStr) && !string.IsNullOrEmpty(prodCantsStr))
            {
                listaIds = prodIdsStr.Split(',').Select(int.Parse).ToList();
                listaCants = prodCantsStr.Split(',').Select(int.Parse).ToList();

                var todosProductos = _productoDAO.ListarTodos();
                for (int i = 0; i < listaIds.Count; i++)
                {
                    var prod = todosProductos.FirstOrDefault(p => p.IdProducto == listaIds[i]);
                    if (prod != null)
                    {
                        total += (prod.Precio * listaCants[i]);
                        listaPrecios.Add(prod.Precio);
                    }
                }
            }

            // Guardamos la venta real en la base de datos usando tu DAO
            int idVenta = _ventaDAO.RegistrarVenta(clienteNombre, clienteDni, total, idFuncion, cantEntradas, asientos, listaIds, listaCants, listaPrecios);

            if (idVenta > 0)
            {
                // Limpiamos la sesión porque la compra ya culminó
                HttpContext.Session.Clear();

                // Redirigimos a la pantalla de éxito mandando el ID generado
                return RedirectToAction("Exito", new { idVenta = idVenta });
            }

            TempData["Error"] = "Hubo un problema al procesar tu compra.";
            return RedirectToAction("ResumenVenta");
        }

        // 6. GET: Pantalla de Éxito Final con el QR
        [HttpGet]
        public IActionResult Exito(int idVenta)
        {
            ViewBag.IdVenta = idVenta;
            return View();
        }
    }
}