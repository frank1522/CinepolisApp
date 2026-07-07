using CinepolisApp.DAO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

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

        // POST: Recibir los combos elegidos y mandar DIRECTO al Resumen de la Boleta
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

            // CALCULAMOS LOS TOTALES PARA EL RESUMEN
            int cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas") ?? 0;
            decimal precioEntrada = 15.00m;
            decimal totalEntradas = cantEntradas * precioEntrada;
            decimal totalCombos = 0;

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
                        totalCombos += (prod.Precio * cants[i]);
                    }
                }
            }

            // Dejamos los montos listos en la sesión para el resumen y la pasarela
            HttpContext.Session.SetString("Carrito_SubtotalEntradas", totalEntradas.ToString("F2"));
            HttpContext.Session.SetString("Carrito_SubtotalSnacks", totalCombos.ToString("F2"));
            HttpContext.Session.SetString("Carrito_TotalPagar", (totalEntradas + totalCombos).ToString("F2"));

            // Te redirige al Resumen de Venta primero
            return RedirectToAction("ResumenVenta", "Compra");
        }

        // 4. GET: Mostrar el Resumen de la Venta (Boleta con la lista de Snacks)
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
                        combosLista.Add(new ComboElegidoDTO { Nombre = prod.Nombre, Cantidad = cants[i], PrecioUnitario = prod.Precio, Subtotal = subtotalItem });
                    }
                }
            }

            ViewBag.Asientos = asientos;
            ViewBag.CantidadEntradas = cantEntradas;
            ViewBag.SubtotalEntradas = totalEntradas.ToString("F2");
            ViewBag.SubtotalSnacks = totalCombos.ToString("F2");
            ViewBag.TotalPagar = (totalEntradas + totalCombos).ToString("F2");
            ViewBag.ListaCombos = combosLista;

            return View();
        }

        // 4.5 POST: Captura el Nombre en el Resumen y salta de inmediato a ConfirmarPago
        [HttpPost]
        public IActionResult EnviarAPasarela(string clienteNombre)
        {
            HttpContext.Session.SetString("Carrito_ClienteNombre", clienteNombre);
            return RedirectToAction("ConfirmarPago", "Compra");
        }

        // 5. GET: Cargar tu Pasarela (ConfirmarPago)
        [HttpGet]
        public IActionResult ConfirmarPago()
        {
            ViewBag.SubtotalEntradas = HttpContext.Session.GetString("Carrito_SubtotalEntradas") ?? "0.00";
            ViewBag.SubtotalSnacks = HttpContext.Session.GetString("Carrito_SubtotalSnacks") ?? "0.00";
            ViewBag.TotalPagar = HttpContext.Session.GetString("Carrito_TotalPagar") ?? "0.00";

            return View(); // Esto levanta tu ConfirmarPago.cshtml
        }

        // 5.5 POST: Procesa la tarjeta, inserta en la BD y manda al Éxito final con el QR
        [HttpPost]
        public IActionResult ProcesarVentaFinal(string dniCliente)
        {
            int idFuncion = HttpContext.Session.GetInt32("Carrito_IdFuncion") ?? 0;
            string asientos = HttpContext.Session.GetString("Carrito_Asientos");
            int cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas") ?? 0;
            decimal total = decimal.Parse(HttpContext.Session.GetString("Carrito_TotalPagar") ?? "0.00");
            string clienteNombre = HttpContext.Session.GetString("Carrito_ClienteNombre") ?? "Cliente Cinépolis";

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
                        listaPrecios.Add(prod.Precio);
                    }
                }
            }

            // Registramos la venta real en la Base de Datos mediante tu DAO
            int idVenta = _ventaDAO.RegistrarVenta(clienteNombre, dniCliente, total, idFuncion, cantEntradas, asientos, listaIds, listaCants, listaPrecios);

            if (idVenta > 0)
            {
                // Pasamos el ID de la venta directo a la vista de éxito
                return RedirectToAction("Exito", new { idVenta = idVenta });
            }

            TempData["Error"] = "Hubo un problema al validar tu método de pago.";
            return RedirectToAction("ConfirmarPago");
        }

        // 6. GET: Pantalla de Éxito Final con el QR simulado
        [HttpGet]
        public IActionResult Exito(int idVenta)
        {
            ViewBag.IdVenta = idVenta;

            // Pasamos los totales finales para que el QR/Boleta cargue limpio
            ViewBag.Asientos = HttpContext.Session.GetString("Carrito_Asientos") ?? "A1";
            ViewBag.CantidadEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas") ?? 1;
            ViewBag.TotalPagar = HttpContext.Session.GetString("Carrito_TotalPagar") ?? "0.00";

            // Limpiamos la sesión porque la compra ya culminó
            HttpContext.Session.Clear();
            return View(); // Esto abre Exito.cshtml (Donde tienes tu QR simulado)
        }

        public class ComboElegidoDTO
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}