USE [master]
GO
/****** Object:  Database [CinepolisDB]    Script Date: 7/07/2026 13:40:00 ******/
CREATE DATABASE [CinepolisDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'CinepolisDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CinepolisDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'CinepolisDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CinepolisDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [CinepolisDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CinepolisDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CinepolisDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CinepolisDB] SET RECOVERY FULL 
GO
ALTER DATABASE [CinepolisDB] SET  MULTI_USER 
GO
ALTER DATABASE [CinepolisDB] SET PAGE_VERIFY CHECKSUM  
GO
USE [CinepolisDB]
GO

/****** Object:  Table [dbo].[Cines] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cines](
	[IdCine] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Direccion] [varchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCine] ASC
)) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Peliculas] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Peliculas](
	[IdPelicula] [int] IDENTITY(1,1) NOT NULL,
	[Titulo] [varchar](150) NOT NULL,
	[Genero] [varchar](50) NOT NULL,
	[Clasificacion] [varchar](30) NOT NULL,
	[Duracion] [int] NOT NULL,
	[ImagenUrl] [varchar](250) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPelicula] ASC
)) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Productos] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Productos](
	[IdProducto] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Descripcion] [varchar](250) NOT NULL,
	[Precio] [decimal](10, 2) NOT NULL,
	[ImagenUrl] [varchar](250) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Funciones] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Funciones](
	[IdFuncion] [int] IDENTITY(1,1) NOT NULL,
	[IdPelicula] [int] NULL,
	[IdCine] [int] NULL,
	[Fecha] [date] NOT NULL,
	[Hora] [varchar](10) NOT NULL,
	[PrecioEntrada] [decimal](10, 2) NOT NULL,
	[Sala] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdFuncion] ASC
)) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Ventas] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ventas](
	[IdVenta] [int] IDENTITY(1,1) NOT NULL,
	[FechaVenta] [datetime] NULL,
	[ClienteNombre] [varchar](100) NOT NULL,
	[ClienteDni] [varchar](8) NOT NULL,
	[Total] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdVenta] ASC
)) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Ventas] ADD  DEFAULT (getdate()) FOR [FechaVenta]
GO

/****** Object:  Table [dbo].[DetalleEntradas] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetalleEntradas](
	[IdDetalleEntrada] [int] IDENTITY(1,1) NOT NULL,
	[IdVenta] [int] NULL,
	[IdFuncion] [int] NULL,
	[Cantidad] [int] NOT NULL,
	[AsientosSeleccionados] [varchar](250) NOT NULL,
	[Subtotal] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalleEntrada] ASC
)) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[DetalleProductos] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetalleProductos](
	[IdDetalleProducto] [int] IDENTITY(1,1) NOT NULL,
	[IdVenta] [int] NULL,
	[IdProducto] [int] NULL,
	[Cantidad] [int] NOT NULL,
	[Subtotal] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDetalleProducto] ASC
)) ON [PRIMARY]
GO

-- LLAVES FORÁNEAS (Relaciones entre tablas)
ALTER TABLE [dbo].[DetalleEntradas]  WITH CHECK ADD FOREIGN KEY([IdFuncion]) REFERENCES [dbo].[Funciones] ([IdFuncion])
GO
ALTER TABLE [dbo].[DetalleEntradas]  WITH CHECK ADD FOREIGN KEY([IdVenta]) REFERENCES [dbo].[Ventas] ([IdVenta])
GO
ALTER TABLE [dbo].[DetalleProductos]  WITH CHECK ADD FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos] ([IdProducto])
GO
ALTER TABLE [dbo].[DetalleProductos]  WITH CHECK ADD FOREIGN KEY([IdVenta]) REFERENCES [dbo].[Ventas] ([IdVenta])
GO
ALTER TABLE [dbo].[Funciones]  WITH CHECK ADD FOREIGN KEY([IdCine]) REFERENCES [dbo].[Cines] ([IdCine])
GO
ALTER TABLE [dbo].[Funciones]  WITH CHECK ADD FOREIGN KEY([IdPelicula]) REFERENCES [dbo].[Peliculas] ([IdPelicula])
GO

-- ==========================================================
-- 🚀 DATA MAESTRA DE INICIALIZACIÓN (IMPRESCINDIBLE PARA TU WEB)
-- ==========================================================

-- 1. Insertar Cines de ejemplo
INSERT INTO [dbo].[Cines] ([Nombre], [Direccion]) VALUES 
('Cinépolis Real Plaza Chiclayo', 'Av. Andrés Avelino Cáceres 222, Chiclayo'),
('Cinépolis Plaza Norte', 'Av. Alfredo Mendiola 1400, Lima');

-- 2. Insertar Películas de cartelera (Con URLs de imágenes locales o de prueba)
INSERT INTO [dbo].[Peliculas] ([Titulo], [Genero], [Clasificacion], [Duracion], [ImagenUrl]) VALUES 
('Deadpool & Wolverine', 'Acción / Comedia', 'Mayores 14', 127, '/images/deadpool.jpg'),
('IntensaMente 2', 'Animación / Familiar', 'Apta Todos', 96, '/images/intensamente.jpg'),
('Mi Villano Favorito 4', 'Animación / Comedia', 'Apta Todos', 94, '/images/minions.jpg');

-- 3. Insertar Combos y Snacks de Dulcería
INSERT INTO [dbo].[Productos] ([Nombre], [Descripcion], [Precio], [ImagenUrl]) VALUES 
('Combo Familiar Super', '2 Canchitas Gigantes + 2 Gaseosas Grandes + 1 Hot Dog', 45.00, '/images/combo1.png'),
('Combo Pareja Clásico', '1 Canchita Grande + 2 Gaseosas Medianas', 28.50, '/images/combo2.png'),
('Canchita Popcorn Sola', '1 Bolsa de Canchita Gigante (Salada/Dulce)', 15.00, '/images/snack1.png'),
('Gaseosa Mediana', 'Vaso de 16oz (Coca-Cola / Inca Kola)', 8.00, '/images/bebida1.png');

-- 4. Insertar Funciones Horarias amarradas a los Cines y Películas
INSERT INTO [dbo].[Funciones] ([IdPelicula], [IdCine], [Fecha], [Hora], [PrecioEntrada], [Sala]) VALUES 
(1, 1, CAST(GETDATE() AS DATE), '15:30', 15.00, 'Sala 02 - 3D'),
(1, 1, CAST(GETDATE() AS DATE), '18:45', 15.00, 'Sala 02 - 3D'),
(2, 1, CAST(GETDATE() AS DATE), '14:00', 15.00, 'Sala 05 - regular'),
(3, 1, CAST(GETDATE() AS DATE), '16:15', 15.00, 'Sala 01 - regular');
GO

USE [master]
GO
ALTER DATABASE [CinepolisDB] SET  READ_WRITE 
GO