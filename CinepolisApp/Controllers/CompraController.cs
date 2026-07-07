using CinepolisApp.DAO;
using Microsoft.AspNetCore.Mvc;

namespace CinepolisApp.Controllers
{
    public class CompraController : Controller
    {
        private readonly FuncionDAO _funcionDAO = new FuncionDAO();

        // Mostrar funciones disponibles para la película seleccionada
        public IActionResult SeleccionarFuncion(int id)
        {
            var funciones = _funcionDAO.ListarPorPelicula(id);

            if (funciones.Count == 0)
            {
                // Si no hay funciones, te devuelve a la cartelera
                return RedirectToAction("Index", "Home");
            }

            // Pasamos la lista de horarios a la vista
            return View(funciones);
        }
        //Mostrar el mapa de asientos (Butacas)
        public IActionResult SeleccionarAsientos(int idFuncion)
        {
            // Pasamos el idFuncion a la vista para saber a qué horario le pertenecen los asientos
            ViewBag.IdFuncion = idFuncion;
            return View();
        }

        // POST: Capturar los asientos seleccionados y guardarlos en Sesión
        [HttpPost]
        public IActionResult GuardarAsientos(int idFuncion, string asientos)
        {
            if (string.IsNullOrEmpty(asientos))
            {
                // Si no seleccionó nada, lo mandamos de vuelta con una alerta
                TempData["Error"] = "Debes seleccionar al menos un asiento.";
                return RedirectToAction("SeleccionarAsientos", new { idFuncion = idFuncion });
            }

            // GUARDAMOS EN LA SESIÓN DEL NAVEGADOR (El carrito empieza a llenarse)
            HttpContext.Session.SetInt32("Carrito_IdFuncion", idFuncion);
            HttpContext.Session.SetString("Carrito_Asientos", asientos);

            // Contamos cuántos asientos eligió (Separados por coma: "A1,A2" = 2 entradas)
            int cantidadEntradas = asientos.Split(',').Length;
            HttpContext.Session.SetInt32("Carrito_CantidadEntradas", cantidadEntradas);

            // Mandamos al usuario al PASO 3: La Dulcería/Snacks (que haremos luego)
            return RedirectToAction("SeleccionarCombos", "Compra");
        }
        private readonly ProductoDAO _productoDAO = new ProductoDAO();

        // GET: Mostrar la dulcería
        public IActionResult SeleccionarCombos()
        {
            // Jalamos los combos de la BD
            var productos = _productoDAO.ListarTodos();
            return View(productos);
        }

        // POST: Recibir los combos elegidos y guardarlos en la Sesión
        [HttpPost]
        public IActionResult GuardarCombos(List<int> idProducto, List<int> cantidad)
        {
            // Creamos dos strings separados por comas para guardar en la sesión simple de .NET Core
            // Ej: "1,2" (IDs de productos) y "2,1" (cantidades)
            List<string> listaIds = new List<string>();
            List<string> listaCants = new List<string>();

            for (int i = 0; i < idProducto.Count; i++)
            {
                if (cantidad[i] > 0) // Solo guardamos los que el usuario sumó
                {
                    listaIds.Add(idProducto[i].ToString());
                    listaCants.Add(cantidad[i].ToString());
                }
            }

            // Guardamos en la sesión (si no eligió nada, se guardará vacío, lo cual es válido)
            HttpContext.Session.SetString("Carrito_ProductosIds", string.Join(",", listaIds));
            HttpContext.Session.SetString("Carrito_ProductosCantidades", string.Join(",", listaCants));

            // Nos vamos al PASO 4: Resumen y Confirmación de la Venta final
            return RedirectToAction("ResumenVenta", "Compra");
        }
        private readonly VentaDAO _ventaDAO = new VentaDAO();

        // GET: Mostrar Resumen del Carrito de Compras
        [HttpGet]
        public IActionResult ResumenVenta()
        {
            // 1. Recuperar datos de Entradas de la Sesión
            int? idFuncion = HttpContext.Session.GetInt32("Carrito_IdFuncion");
            string asientos = HttpContext.Session.GetString("Carrito_Asientos");
            int? cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas");

            if (idFuncion == null || string.IsNullOrEmpty(asientos))
            {
                return RedirectToAction("Index", "Home");
            }

            decimal precioEntrada = 15.00m; // Precio base por entrada
            decimal totalEntradas = (cantEntradas ?? 0) * precioEntrada;
            decimal totalCombos = 0;

            // Crear una lista temporal para pasar los combos elegidos a la vista
            List<ComboElegidoDTO> combosLista = new List<ComboElegidoDTO>();

            // 2. Recuperar datos de la Dulcería de la Sesión
            string prodIdsStr = HttpContext.Session.GetString("Carrito_ProductosIds");
            string prodCantsStr = HttpContext.Session.GetString("Carrito_ProductosCantidades");

            if (!string.IsNullOrEmpty(prodIdsStr) && !string.IsNullOrEmpty(prodCantsStr))
            {
                var ids = prodIdsStr.Split(',').Select(int.Parse).ToList();
                var cants = prodCantsStr.Split(',').Select(int.Parse).ToList();

                // Jalamos todos los productos para comparar precios
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

            // 3. Pasar absolutamente todo al ViewBags
            ViewBag.Asientos = asientos;
            ViewBag.CantidadEntradas = cantEntradas;
            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.ListaCombos = combosLista;
            ViewBag.TotalCombos = totalCombos;

            // El gran total sumando ambos mundos
            ViewBag.TotalPagar = totalEntradas + totalCombos;

            return View();
        }

        // Objeto temporal para estructurar los combos en el resumen
        public class ComboElegidoDTO
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
        }

        // POST: Procesar el Botón de Pago Final
        [HttpPost]
        public IActionResult TerminarCompra(string clienteNombre, string clienteDni)
        {
            int idFuncion = HttpContext.Session.GetInt32("Carrito_IdFuncion") ?? 0;
            string asientos = HttpContext.Session.GetString("Carrito_Asientos");
            int cantEntradas = HttpContext.Session.GetInt32("Carrito_CantidadEntradas") ?? 0;

            decimal precioEntrada = 15.00m;
            decimal total = cantEntradas * precioEntrada;

            // Recalcular los combos para sumarlos al total final de la BD
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

            // Pasamos las listas completas al VentaDAO para que registre la cabecera y el DetalleProductos
            int idVenta = _ventaDAO.RegistrarVenta(clienteNombre, clienteDni, total, idFuncion, cantEntradas, asientos, listaIds, listaCants, listaPrecios);

            if (idVenta > 0)
            {
                // Guardamos el ID de la venta en una variable temporal para el siguiente paso (El PDF/Ticket)
                TempData["MensajeExito"] = $"¡Compra Exitosa! Tu código de Ticket es N° 000{idVenta}";
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Hubo un problema al procesar tu compra.";
            return RedirectToAction("ResumenVenta");
        }
    }
}