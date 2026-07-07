# 🎬 CinepolisApp - Sistema de Reserva y Compra de Entradas

Este es un proyecto web desarrollado en **.NET 8.0** usando la arquitectura **ASP.NET Core MVC**. El sistema simula el flujo completo de compra de entradas y combos de dulcería para una cadena de cines, manejando almacenamiento temporal en memoria (Sesiones) y persistencia en Base de Datos.

## 🚀 Características y Módulos
* **Cartelera Principal:** Listado dinámico de películas directo desde la base de datos.
* **Selección de Funciones y Horarios:** Filtrado de horarios asignados por película y cine.
* **Mapa de Butacas Interactivo:** Selección de asientos en tiempo real con cálculo automático de tickets.
* **Dulcería Integral:** Módulo de adición de snacks y combos con gestión de subtotales.
* **Pasarela de Pago Segura (Simulada):** Soporte multi-método para pagos con Tarjeta de Crédito/Débito y códigos de aprobación de Yape/Plin.
* **Generación de Ticket con QR:** Interfaz final local que simula la entrega de un código QR de reserva único basado en el ID de la transacción.

## 🛠️ Tecnologías Utilizadas
* **Backend:** C# / .NET 8.0 / ASP.NET Core MVC
* **Persistencia:** SQL Server (Arquitectura DAO con Conexión Nativa ADO.NET)
* **Estado Temporal:** `HttpContext.Session` (Sesiones distribuidas activas en memoria)
* **Frontend:** Razor Pages, HTML5, CSS3, Bootstrap 5, FontAwesome

## 📂 Estructura del Proyecto
* `Controllers/`: Lógica de control del flujo (`CompraController.cs`, `HomeController.cs`).
* `DAO/`: Data Access Objects para la comunicación directa con SQL Server.
* `Views/`: Vistas Razor estructuradas para el usuario (`ResumenVenta.cshtml`, `ConfirmarPago.cshtml`, `Exito.cshtml`).
* `wwwroot/`: Archivos estáticos, estilos CSS, imágenes de películas y recursos locales (como el QR estático).

## 🔧 Configuración del Proyecto

### 1. Base de Datos
1. Abre SQL Server Management Studio (SSMS).
2. Ejecuta el script de inicialización ubicado en la raíz: `CinepolisDB.sql`.
*Nota: Este script incluye el esquema completo del sistema y los datos maestros necesarios (películas y productos). Las tablas de transacciones se inicializan vacías para pruebas locales.*

### 2. Configurar la Conexión en la App
Asegúrate de revisar tu cadena de conexión en los archivos DAO o en el `appsettings.json` para que apunte a tu servidor local:
```csharp
"Server=TU_SERVIDOR_LOCAL; Database=CinepolisDB; Trusted_Connection=True;"