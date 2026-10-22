-- ============================================================
-- BASE DE DATOS VIRGEN: sistema_gastronomico
-- Preparada para importar desde phpMyAdmin (pestaña Importar).
-- Conserva admin_1, las tablas Rol y Categoria, el empleado
-- relacionado necesario para la FK, 10 proveedores y 55 productos.
-- Las demás tablas quedan vacías.
-- ============================================================
CREATE DATABASE IF NOT EXISTS `sistema_gastronomico`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE `sistema_gastronomico`;

-- BASE DE DATOS VIRGEN: conserva admin_1, roles y categorías; carga 10 proveedores y 55 productos.
-- El empleado vinculado a admin_1 se conserva porque es requerido por la clave foránea.
-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: localhost    Database: sistema_gastronomico
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `categoria`
--

DROP TABLE IF EXISTS `categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categoria` (
  `id_categoria` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_categoria`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categoria`
--

LOCK TABLES `categoria` WRITE;
/*!40000 ALTER TABLE `categoria` DISABLE KEYS */;
INSERT INTO `categoria` VALUES (1,'Entrada',1),(2,'Plato Principal',1),(3,'Postre',1),(4,'Bebida',1),(5,'Otro',1);
/*!40000 ALTER TABLE `categoria` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `compra`
--

DROP TABLE IF EXISTS `compra`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `compra` (
  `id_compra` int NOT NULL AUTO_INCREMENT,
  `fecha_hora` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `numero_comprobante` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `total_compra` decimal(10,2) NOT NULL DEFAULT '0.00',
  `id_empleado` int NOT NULL,
  `estado` tinyint(1) NOT NULL DEFAULT '1',
  `id_proveedor` int NOT NULL,
  PRIMARY KEY (`id_compra`),
  UNIQUE KEY `uk_proveedor_comprobante` (`id_proveedor`,`numero_comprobante`),
  KEY `fk_compra_empleado` (`id_empleado`),
  KEY `idx_id_proveedor` (`id_proveedor`),
  CONSTRAINT `fk_compra_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleado` (`id_empleado`) ON UPDATE CASCADE,
  CONSTRAINT `fk_compra_proveedor` FOREIGN KEY (`id_proveedor`) REFERENCES `proveedor` (`id_proveedor`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `compra`
--

LOCK TABLES `compra` WRITE;
/*!40000 ALTER TABLE `compra` DISABLE KEYS */;

/*!40000 ALTER TABLE `compra` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `detalle_compra`
--

DROP TABLE IF EXISTS `detalle_compra`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `detalle_compra` (
  `id_detalle_compra` int NOT NULL AUTO_INCREMENT,
  `id_compra` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad_ingresada` decimal(10,3) NOT NULL,
  `precio_costo_unitario` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) GENERATED ALWAYS AS ((`cantidad_ingresada` * `precio_costo_unitario`)) STORED,
  PRIMARY KEY (`id_detalle_compra`),
  KEY `fk_detalle_compra_compra` (`id_compra`),
  KEY `fk_detalle_compra_producto` (`id_producto`),
  CONSTRAINT `fk_detalle_compra_compra` FOREIGN KEY (`id_compra`) REFERENCES `compra` (`id_compra`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_detalle_compra_producto` FOREIGN KEY (`id_producto`) REFERENCES `producto` (`id_producto`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_compra`
--

LOCK TABLES `detalle_compra` WRITE;
/*!40000 ALTER TABLE `detalle_compra` DISABLE KEYS */;

/*!40000 ALTER TABLE `detalle_compra` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `detalle_pedido`
--

DROP TABLE IF EXISTS `detalle_pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `detalle_pedido` (
  `id_detalle_pedido` int NOT NULL AUTO_INCREMENT,
  `id_pedido` int NOT NULL,
  `id_plato` int NOT NULL,
  `cantidad` int NOT NULL DEFAULT '1',
  `precio_unitario` decimal(10,2) NOT NULL,
  `estado` enum('EnMarcha','Despachado') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'EnMarcha',
  `fecha_hora` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_detalle_pedido`),
  KEY `fk_detalle_pedido` (`id_pedido`),
  KEY `fk_detalle_plato` (`id_plato`),
  CONSTRAINT `fk_detalle_pedido` FOREIGN KEY (`id_pedido`) REFERENCES `pedido` (`id_pedido`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_detalle_plato` FOREIGN KEY (`id_plato`) REFERENCES `plato` (`id_plato`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_pedido`
--

LOCK TABLES `detalle_pedido` WRITE;
/*!40000 ALTER TABLE `detalle_pedido` DISABLE KEYS */;

/*!40000 ALTER TABLE `detalle_pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `detalle_receta`
--

DROP TABLE IF EXISTS `detalle_receta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `detalle_receta` (
  `id_detalle_receta` int NOT NULL AUTO_INCREMENT,
  `id_plato` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad_requerida` decimal(10,3) NOT NULL,
  PRIMARY KEY (`id_detalle_receta`),
  UNIQUE KEY `uq_plato_producto` (`id_plato`,`id_producto`),
  KEY `fk_receta_producto` (`id_producto`),
  CONSTRAINT `fk_receta_plato` FOREIGN KEY (`id_plato`) REFERENCES `plato` (`id_plato`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_receta_producto` FOREIGN KEY (`id_producto`) REFERENCES `producto` (`id_producto`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=56 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `detalle_receta`
--

LOCK TABLES `detalle_receta` WRITE;
/*!40000 ALTER TABLE `detalle_receta` DISABLE KEYS */;
/*!40000 ALTER TABLE `detalle_receta` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empleado`
--

DROP TABLE IF EXISTS `empleado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empleado` (
  `id_empleado` int NOT NULL AUTO_INCREMENT,
  `legajo` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellido` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `dni` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `telefono` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha_ingreso` date NOT NULL,
  `activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_empleado`),
  UNIQUE KEY `legajo` (`legajo`),
  UNIQUE KEY `dni` (`dni`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleado`
--

LOCK TABLES `empleado` WRITE;
/*!40000 ALTER TABLE `empleado` DISABLE KEYS */;
INSERT INTO `empleado` VALUES (2,'0001','Leandro','Leyes','44358717','123456567','2026-10-12',1);
/*!40000 ALTER TABLE `empleado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mesa`
--

DROP TABLE IF EXISTS `mesa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mesa` (
  `id_mesa` int NOT NULL AUTO_INCREMENT,
  `numero` int NOT NULL,
  `capacidad` int NOT NULL DEFAULT '2',
  `estado` tinyint(1) NOT NULL DEFAULT '0',
  `tipo` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_mesa`),
  UNIQUE KEY `numero` (`numero`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mesa`
--

LOCK TABLES `mesa` WRITE;
/*!40000 ALTER TABLE `mesa` DISABLE KEYS */;

/*!40000 ALTER TABLE `mesa` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedido`
--

DROP TABLE IF EXISTS `pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pedido` (
  `id_pedido` int NOT NULL AUTO_INCREMENT,
  `id_mesa` int NOT NULL,
  `id_empleado` int NOT NULL,
  `fecha_hora` datetime DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('Abierto','Pagado','Cancelado') COLLATE utf8mb4_unicode_ci DEFAULT 'Abierto',
  `total` decimal(10,2) DEFAULT '0.00',
  PRIMARY KEY (`id_pedido`),
  KEY `fk_pedido_mesa` (`id_mesa`),
  KEY `fk_pedido_empleado` (`id_empleado`),
  CONSTRAINT `fk_pedido_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleado` (`id_empleado`) ON UPDATE CASCADE,
  CONSTRAINT `fk_pedido_mesa` FOREIGN KEY (`id_mesa`) REFERENCES `mesa` (`id_mesa`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedido`
--

LOCK TABLES `pedido` WRITE;
/*!40000 ALTER TABLE `pedido` DISABLE KEYS */;

/*!40000 ALTER TABLE `pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `plato`
--

DROP TABLE IF EXISTS `plato`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `plato` (
  `id_plato` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(120) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_categoria` int NOT NULL,
  `precio_venta` decimal(10,2) NOT NULL,
  `activo` tinyint(1) DEFAULT '1',
  `imagen` varchar(200) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id_plato`),
  KEY `fk_plato_categoria` (`id_categoria`),
  CONSTRAINT `fk_plato_categoria` FOREIGN KEY (`id_categoria`) REFERENCES `categoria` (`id_categoria`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `plato`
--

LOCK TABLES `plato` WRITE;
/*!40000 ALTER TABLE `plato` DISABLE KEYS */;

/*!40000 ALTER TABLE `plato` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `producto`
--

DROP TABLE IF EXISTS `producto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `producto` (
  `id_producto` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `cantidad_stock` decimal(10,3) NOT NULL DEFAULT '0.000',
  `unidad_medida` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `precio_costo` decimal(10,2) NOT NULL DEFAULT '0.00',
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_producto`),
  UNIQUE KEY `uq_producto_nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` (`id_producto`,`nombre`,`cantidad_stock`,`unidad_medida`,`precio_costo`,`estado`) VALUES
(1, 'Harina 000', 25.000, 'kg', 950.00, 1),
(2, 'Harina leudante', 10.000, 'kg', 1250.00, 1),
(3, 'Azúcar', 15.000, 'kg', 1100.00, 1),
(4, 'Sal fina', 5.000, 'kg', 650.00, 1),
(5, 'Aceite de girasol', 12.000, 'litro', 2200.00, 1),
(6, 'Aceite de oliva', 3.000, 'litro', 8500.00, 1),
(7, 'Arroz largo fino', 15.000, 'kg', 1400.00, 1),
(8, 'Fideos secos', 20.000, 'kg', 1250.00, 1),
(9, 'Pan rallado', 8.000, 'kg', 1700.00, 1),
(10, 'Levadura seca', 2.000, 'kg', 7800.00, 1),
(11, 'Leche entera', 24.000, 'litro', 1250.00, 1),
(12, 'Crema de leche', 8.000, 'litro', 4800.00, 1),
(13, 'Manteca', 5.000, 'kg', 7200.00, 1),
(14, 'Queso cremoso', 12.000, 'kg', 6500.00, 1),
(15, 'Queso mozzarella', 8.000, 'kg', 7800.00, 1),
(16, 'Queso rallado', 3.000, 'kg', 9500.00, 1),
(17, 'Huevos', 15.000, 'docena', 2800.00, 1),
(18, 'Carne vacuna', 20.000, 'kg', 9800.00, 1),
(19, 'Carne picada', 12.000, 'kg', 7500.00, 1),
(20, 'Pollo', 18.000, 'kg', 4200.00, 1),
(21, 'Jamón cocido', 5.000, 'kg', 6800.00, 1),
(22, 'Panceta', 4.000, 'kg', 8900.00, 1),
(23, 'Chorizo', 8.000, 'kg', 5300.00, 1),
(24, 'Pescado filet', 6.000, 'kg', 7200.00, 1),
(25, 'Tomate', 15.000, 'kg', 2400.00, 1),
(26, 'Cebolla', 12.000, 'kg', 1200.00, 1),
(27, 'Papa', 25.000, 'kg', 950.00, 1),
(28, 'Zanahoria', 8.000, 'kg', 1100.00, 1),
(29, 'Morrón rojo', 5.000, 'kg', 3200.00, 1),
(30, 'Lechuga', 6.000, 'kg', 1500.00, 1),
(31, 'Ajo', 2.000, 'kg', 2800.00, 1),
(32, 'Limón', 8.000, 'kg', 1300.00, 1),
(33, 'Manzana', 10.000, 'kg', 2200.00, 1),
(34, 'Banana', 10.000, 'kg', 1800.00, 1),
(35, 'Frutilla', 4.000, 'kg', 4500.00, 1),
(36, 'Palta', 4.000, 'kg', 5500.00, 1),
(37, 'Puré de tomate', 24.000, 'unidad', 950.00, 1),
(38, 'Mayonesa', 6.000, 'kg', 3800.00, 1),
(39, 'Mostaza', 3.000, 'kg', 3200.00, 1),
(40, 'Ketchup', 4.000, 'kg', 3500.00, 1),
(41, 'Vinagre', 6.000, 'litro', 1300.00, 1),
(42, 'Caldo concentrado', 2.000, 'kg', 6500.00, 1),
(43, 'Orégano', 1.000, 'kg', 9000.00, 1),
(44, 'Pimienta negra', 1.000, 'kg', 12500.00, 1),
(45, 'Pimentón', 1.000, 'kg', 8500.00, 1),
(46, 'Chocolate cobertura', 4.000, 'kg', 9800.00, 1),
(47, 'Cacao en polvo', 3.000, 'kg', 7200.00, 1),
(48, 'Dulce de leche', 8.000, 'kg', 4200.00, 1),
(49, 'Gelatina', 30.000, 'unidad', 650.00, 1),
(50, 'Café molido', 5.000, 'kg', 9500.00, 1),
(51, 'Té en saquitos', 20.000, 'caja', 1100.00, 1),
(52, 'Agua mineral', 24.000, 'litro', 700.00, 1),
(53, 'Gaseosa cola', 24.000, 'litro', 1800.00, 1),
(54, 'Jugo en polvo', 50.000, 'unidad', 300.00, 1),
(55, 'Pan para hamburguesa', 40.000, 'unidad', 450.00, 1);
/*!40000 ALTER TABLE `producto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `proveedor`
--

DROP TABLE IF EXISTS `proveedor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `proveedor` (
  `id_proveedor` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `cuit` varchar(13) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(30) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `direccion` varchar(150) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `estado` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_proveedor`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `proveedor`
--

LOCK TABLES `proveedor` WRITE;
/*!40000 ALTER TABLE `proveedor` DISABLE KEYS */;
INSERT INTO `proveedor` (`id_proveedor`,`nombre`,`cuit`,`telefono`,`direccion`,`estado`) VALUES
(1, 'Verdulería La Quinta', '30-71111111-1', '2665407728', 'Los Lapachos 123', 1),
(2, 'Carnicería La Brava', '30-72222222-2', '2656475444', 'Siempre Viva 123', 1),
(3, 'Distribuidora J&M', '30-73333333-3', '2665411444', 'San Martín 232', 1),
(4, 'Lácteos del Valle', '30-74444444-4', '2664551234', 'Ruta 3 km 5', 1),
(5, 'Panificados San Luis', '30-75555555-5', '2664778899', 'Pringles 450', 1),
(6, 'Granja El Molino', '30-76666666-6', '2664881122', 'Belgrano 780', 1),
(7, 'Almacén Mayorista Centro', '30-77777777-7', '2664123456', 'Colón 1020', 1),
(8, 'Bebidas Cuyo', '30-78888888-8', '2664332211', 'Junín 615', 1),
(9, 'Congelados del Sur', '30-79999999-9', '2664998877', 'Riobamba 340', 1),
(10, 'Dulcería La Central', '30-70000000-0', '2664223344', 'Lavalle 890', 1);
/*!40000 ALTER TABLE `proveedor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rol`
--

DROP TABLE IF EXISTS `rol`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rol` (
  `id_rol` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_rol`),
  UNIQUE KEY `nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rol`
--

LOCK TABLES `rol` WRITE;
/*!40000 ALTER TABLE `rol` DISABLE KEYS */;
INSERT INTO `rol` VALUES (1,'Administrador'),(3,'Cajero'),(2,'Mozo');
/*!40000 ALTER TABLE `rol` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario`
--

DROP TABLE IF EXISTS `usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `id_empleado` int NOT NULL,
  `avatar` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `id_rol` int NOT NULL,
  `username` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `id_empleado` (`id_empleado`),
  UNIQUE KEY `username` (`username`),
  KEY `fk_usuario_rol` (`id_rol`),
  CONSTRAINT `fk_usuario_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleado` (`id_empleado`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_usuario_rol` FOREIGN KEY (`id_rol`) REFERENCES `rol` (`id_rol`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (2,2,'/uploads/avatars/2_7077f5b10e95418696d14d7194f8c1bf.jpg',1,'admin_1','AQAAAAIAAYagAAAAEOb6hpfKSQUHw+eQ3rydjExZRj6oe10kcRmsycMS4JUXV2c/RH4FQPyAIV7GJEiK4g==',1);
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'sistema_gastronomico'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-22 16:52:44
