using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using sistema_gastronomico.Tests.Infraestructura;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Controllers;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using Xunit;

namespace sistema_gastronomico.Tests
{
    // Pruebas de las validaciones que viven en los CONTROLADORES: las que necesitan consultar
    // la base (duplicados), la receta del plato y el mozo del pedido.
    //
    // Importante: al llamar una acción directamente desde una prueba, MVC NO ejecuta el model
    // binding, así que los atributos del modelo ([RegularExpression], [StringLength], etc.) no
    // se evalúan acá; esos se prueban en ValidacionModelosTests. Acá se usan datos con formato
    // válido para aislar la lógica del controlador.
    //
    // Cada prueba usa una base en memoria nueva (Prueba.CrearContexto), en lugar de MySQL.
    public class ValidacionControladoresTests
    {
        // =====================================================================
        // Producto: nombre duplicado
        // =====================================================================

        private static ProductoController CrearProductoController(GastronomiaContext ctx)
            => Prueba.Preparar(new ProductoController(new RepositorioProducto(ctx), NullLogger<ProductoController>.Instance));

        private static Producto NuevoProducto(string nombre) => new()
        {
            Nombre = nombre,
            Cantidad_stock = 10,
            Unidad_medida = "kg",
            Precio_costo = 100
        };

        [Fact]
        public void Producto_Crear_ConNombreDeUnoActivo_DevuelveErrorEnNombre()
        {
            // Arrange
            using var ctx = Prueba.CrearContexto();
            ctx.Productos.Add(new Producto { Nombre = "Harina 000", Cantidad_stock = 5, Unidad_medida = "kg", Precio_costo = 950, Estado = true });
            ctx.SaveChanges();
            var controlador = CrearProductoController(ctx);

            // Act
            var resultado = controlador.Crear(NuevoProducto("Harina 000"));

            // Assert
            var vista = Assert.IsType<ViewResult>(resultado);
            Assert.Equal("Alta", vista.ViewName);
            Assert.True(controlador.ModelState.ContainsKey(nameof(Producto.Nombre)));
            Assert.Equal(1, ctx.Productos.Count());
        }

        [Fact]
        public void Producto_Crear_ConNombreDeUnoDadoDeBaja_AvisaQueEstaDadoDeBaja()
        {
            using var ctx = Prueba.CrearContexto();
            ctx.Productos.Add(new Producto { Nombre = "Harina 000", Cantidad_stock = 5, Unidad_medida = "kg", Precio_costo = 950, Estado = false });
            ctx.SaveChanges();
            var controlador = CrearProductoController(ctx);

            controlador.Crear(NuevoProducto("Harina 000"));

            var mensaje = controlador.ModelState[nameof(Producto.Nombre)]!.Errors.Single().ErrorMessage;
            Assert.Contains("dado de baja", mensaje);
        }

        [Fact]
        public void Producto_Crear_ConNombreNuevo_Guarda()
        {
            using var ctx = Prueba.CrearContexto();
            var controlador = CrearProductoController(ctx);

            var resultado = controlador.Crear(NuevoProducto("  Azúcar  "));

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Azúcar", ctx.Productos.Single().Nombre);  // se guarda sin espacios sobrantes
        }

        [Fact]
        public void Producto_Modificar_ConSuPropioNombre_NoLoTomaComoDuplicado()
        {
            using var ctx = Prueba.CrearContexto();
            var existente = new Producto { Nombre = "Harina 000", Cantidad_stock = 5, Unidad_medida = "kg", Precio_costo = 950, Estado = true };
            ctx.Productos.Add(existente);
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();  // simula un request nuevo
            var controlador = CrearProductoController(ctx);

            var modificado = NuevoProducto("Harina 000");
            modificado.IdProducto = existente.IdProducto;
            modificado.Precio_costo = 1200;
            modificado.Estado = true;
            controlador.Modificar(modificado);

            Assert.False(controlador.ModelState.ContainsKey(nameof(Producto.Nombre)));
        }

        // =====================================================================
        // Plato: nombre duplicado y receta
        // =====================================================================

        private static PlatoController CrearPlatoController(GastronomiaContext ctx)
            => Prueba.Preparar(new PlatoController(
                new RepositorioPlato(ctx),
                new RepositorioDetalleReceta(ctx),
                new RepositorioCategoria(ctx),
                new RepositorioProducto(ctx),
                ctx,
                null!,  // IWebHostEnvironment: solo se usa para imágenes, que estas pruebas no suben
                NullLogger<PlatoController>.Instance));

        // Carga una categoría y tres insumos (el 3 está dado de baja).
        private static void CargarDatosPlato(GastronomiaContext ctx)
        {
            ctx.Categorias.Add(new Categoria { IdCategoria = 1, Nombre = "Principales", Estado = true });
            ctx.Productos.AddRange(
                new Producto { IdProducto = 1, Nombre = "Carne", Cantidad_stock = 10, Unidad_medida = "kg", Precio_costo = 100, Estado = true },
                new Producto { IdProducto = 2, Nombre = "Papa", Cantidad_stock = 10, Unidad_medida = "kg", Precio_costo = 50, Estado = true },
                new Producto { IdProducto = 3, Nombre = "Insumo viejo", Cantidad_stock = 0, Unidad_medida = "kg", Precio_costo = 10, Estado = false });
            ctx.SaveChanges();
        }

        private static PlatoFormViewModel NuevoPlato(string nombre, params (int idProducto, decimal cantidad)[] receta) => new()
        {
            Plato = new Plato { Nombre = nombre, PrecioVenta = 9500, IdCategoria = 1 },
            Receta = receta.Select(r => new DetalleRecetaFormItem { IdProducto = r.idProducto, CantidadRequerida = r.cantidad }).ToList()
        };

        [Fact]
        public async Task Plato_Crear_ConRecetaValida_GuardaPlatoYReceta()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m), (2, 0.3m)));

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Single(ctx.Platos);
            Assert.Equal(2, ctx.DetalleRecetas.Count());
        }

        [Fact]
        public async Task Plato_Crear_SinIngredientes_DevuelveError()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            // La vista agrega una fila vacía por defecto: no cuenta como ingrediente.
            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (0, 0m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.Contains(controlador.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("al menos un ingrediente"));
            Assert.Empty(ctx.Platos);
        }

        [Fact]
        public async Task Plato_Crear_ConInsumoRepetido_DevuelveErrorYNoGuardaNada()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m), (1, 0.10m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.Contains(controlador.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("repetido"));
            Assert.Empty(ctx.Platos);  // antes quedaba el plato creado sin receta
        }

        [Fact]
        public async Task Plato_Crear_ConCantidadCero_DevuelveError()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.Contains(controlador.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("cantidad"));
        }

        [Fact]
        public async Task Plato_Crear_ConCantidadSinInsumo_DevuelveError()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m), (0, 2m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.Contains(controlador.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("no se eligió el insumo"));
        }

        [Fact]
        public async Task Plato_Crear_ConInsumoDadoDeBaja_DevuelveError()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (3, 1m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.Contains(controlador.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("dado de baja"));
        }

        [Fact]
        public async Task Plato_Crear_ConErrores_DevuelveLosNombresDeLosInsumos()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m), (1, 0.10m)));

            // Sin esto el select2 de la receta aparecía vacío al volver al formulario.
            var vm = Assert.IsType<PlatoFormViewModel>(Assert.IsType<ViewResult>(resultado).Model);
            Assert.All(vm.Receta, r => Assert.Equal("Carne", r.NombreProducto));
        }

        [Fact]
        public async Task Plato_Crear_ConNombreDeOtroActivo_DevuelveErrorEnNombre()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            ctx.Platos.Add(new Plato { Nombre = "Milanesa", PrecioVenta = 9000, IdCategoria = 1, Estado = true });
            ctx.SaveChanges();
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m)));

            Assert.IsType<ViewResult>(resultado);
            Assert.True(controlador.ModelState.ContainsKey("Plato.Nombre"));
        }

        [Fact]
        public async Task Plato_Crear_ConNombreDeUnoDadoDeBaja_Guarda()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosPlato(ctx);
            ctx.Platos.Add(new Plato { Nombre = "Milanesa", PrecioVenta = 9000, IdCategoria = 1, Estado = false });
            ctx.SaveChanges();
            var controlador = CrearPlatoController(ctx);

            var resultado = await controlador.Crear(NuevoPlato("Milanesa", (1, 0.25m)));

            Assert.IsType<RedirectToActionResult>(resultado);
        }

        // =====================================================================
        // Proveedor: CUIT duplicado
        // =====================================================================

        private static ProveedorController CrearProveedorController(GastronomiaContext ctx)
            => Prueba.Preparar(new ProveedorController(new RepositorioProveedor(ctx), NullLogger<ProveedorController>.Instance));

        private static Proveedor NuevoProveedor(string cuit) => new()
        {
            Nombre = "Proveedor Nuevo",
            Cuit = cuit,
            Telefono = "2664123456"
        };

        [Fact]
        public void Proveedor_Alta_ConCuitDeOtroActivo_DevuelveErrorEnCuit()
        {
            using var ctx = Prueba.CrearContexto();
            ctx.Proveedores.Add(new Proveedor { Nombre = "La Quinta", Cuit = "30-71111111-1", Telefono = "2665407728", Estado = true });
            ctx.SaveChanges();
            var controlador = CrearProveedorController(ctx);

            var resultado = controlador.Alta(NuevoProveedor("30-71111111-1"));

            Assert.IsType<ViewResult>(resultado);
            Assert.True(controlador.ModelState.ContainsKey(nameof(Proveedor.Cuit)));
            Assert.Single(ctx.Proveedores);
        }

        [Fact]
        public void Proveedor_Alta_ConCuitDeUnoDadoDeBaja_Guarda()
        {
            using var ctx = Prueba.CrearContexto();
            ctx.Proveedores.Add(new Proveedor { Nombre = "La Quinta", Cuit = "30-71111111-1", Telefono = "2665407728", Estado = false });
            ctx.SaveChanges();
            var controlador = CrearProveedorController(ctx);

            var resultado = controlador.Alta(NuevoProveedor("30-71111111-1"));

            Assert.IsType<RedirectToActionResult>(resultado);
        }

        [Fact]
        public void Proveedor_Editar_ConSuPropioCuit_Guarda()
        {
            using var ctx = Prueba.CrearContexto();
            var existente = new Proveedor { Nombre = "La Quinta", Cuit = "30-71111111-1", Telefono = "2665407728", Estado = true };
            ctx.Proveedores.Add(existente);
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();
            var controlador = CrearProveedorController(ctx);

            var editado = NuevoProveedor("30-71111111-1");
            editado.Nombre = "La Quinta SRL";
            var resultado = controlador.Editar(existente.IdProveedor, editado);

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("La Quinta SRL", ctx.Proveedores.Single().Nombre);
        }

        // =====================================================================
        // Compra: comprobante repetido por proveedor
        // =====================================================================

        private static CompraController CrearCompraController(GastronomiaContext ctx, int idEmpleado = 1)
            => Prueba.Preparar(new CompraController(
                new RepositorioCompra(ctx),
                new RepositorioProducto(ctx),
                new RepositorioProveedor(ctx),
                NullLogger<CompraController>.Instance), idEmpleado);

        private static void CargarDatosCompra(GastronomiaContext ctx)
        {
            ctx.Proveedores.AddRange(
                new Proveedor { IdProveedor = 1, Nombre = "La Quinta", Cuit = "30-71111111-1", Telefono = "2665407728", Estado = true },
                new Proveedor { IdProveedor = 2, Nombre = "La Brava", Cuit = "30-72222222-5", Telefono = "2656475444", Estado = true });
            ctx.Productos.Add(new Producto { IdProducto = 1, Nombre = "Harina", Cantidad_stock = 0, Unidad_medida = "kg", Precio_costo = 100, Estado = true });
            ctx.Compras.Add(new Compra { IdProveedor = 1, IdEmpleado = 1, NumeroComprobante = "FC-0001-1234", FechaHora = DateTime.Now, TotalCompra = 100, Estado = true });
            ctx.SaveChanges();
        }

        private static Compra NuevaCompra(int idProveedor, string? comprobante) => new()
        {
            IdProveedor = idProveedor,
            NumeroComprobante = comprobante,
            FechaHora = DateTime.Now,
            Detalles = new List<DetalleCompra>
            {
                new() { IdProducto = 1, CantidadIngresada = 5, PrecioCostoUnitario = 100 }
            }
        };

        [Fact]
        public void Compra_Alta_ConComprobanteRepetidoDelMismoProveedor_DevuelveError()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosCompra(ctx);
            var controlador = CrearCompraController(ctx);

            var resultado = controlador.Alta(NuevaCompra(1, "fc-0001-1234"));  // se normaliza a mayúsculas

            Assert.IsType<ViewResult>(resultado);
            Assert.True(controlador.ModelState.ContainsKey(nameof(Compra.NumeroComprobante)));
            Assert.Single(ctx.Compras);
        }

        [Fact]
        public void Compra_Alta_ConMismoComprobanteDeOtroProveedor_Guarda()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosCompra(ctx);
            var controlador = CrearCompraController(ctx);

            var resultado = controlador.Alta(NuevaCompra(2, "FC-0001-1234"));

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal(2, ctx.Compras.Count());
        }

        [Fact]
        public void Compra_Alta_SiempreUsaElEmpleadoLogueado()
        {
            using var ctx = Prueba.CrearContexto();
            CargarDatosCompra(ctx);
            var controlador = CrearCompraController(ctx, idEmpleado: 7);

            var compra = NuevaCompra(2, null);
            compra.IdEmpleado = 999;  // valor manipulado en el formulario
            controlador.Alta(compra);

            Assert.Contains(ctx.Compras, c => c.IdProveedor == 2 && c.IdEmpleado == 7);
        }

        // =====================================================================
        // Usuario: legajo, DNI y username duplicados
        // =====================================================================

        private static UsuarioController CrearUsuarioController(GastronomiaContext ctx)
            => Prueba.Preparar(new UsuarioController(
                new RepositorioUsuario(ctx),
                new RepositorioEmpleado(ctx),
                new RepositorioRol(ctx),
                ctx,
                new PasswordHasher<Usuario>(),
                null!));  // IWebHostEnvironment: solo se usa para el avatar

        private static void CargarUsuarioExistente(GastronomiaContext ctx)
        {
            ctx.Roles.Add(new Rol { IdRol = 1, Nombre = "Administrador" });
            ctx.Empleados.Add(new Empleado { IdEmpleado = 1, Legajo = "0001", Nombre = "Leandro", Apellido = "Leyes", Dni = "44358717", Fecha_ingreso = DateTime.Today, Activo = true });
            ctx.Usuarios.Add(new Usuario { IdUsuario = 1, IdEmpleado = 1, IdRol = 1, Username = "admin_1", PasswordHash = "x", Estado = true });
            ctx.SaveChanges();
        }

        private static RegistrarUsuarioViewModel NuevoRegistro() => new()
        {
            Legajo = "0002",
            Nombre = "Ana",
            Apellido = "Gómez",
            Dni = "30111222",
            FechaIngreso = DateTime.Today,
            IdRol = 1,
            Username = "ana.gomez",
            Password = "clave123",
            ConfirmPassword = "clave123"
        };

        [Theory]
        [InlineData(nameof(RegistrarUsuarioViewModel.Legajo))]
        [InlineData(nameof(RegistrarUsuarioViewModel.Dni))]
        [InlineData(nameof(RegistrarUsuarioViewModel.Username))]
        public void Usuario_Registrar_ConDatoDuplicado_DevuelveErrorEnEseCampo(string campo)
        {
            using var ctx = Prueba.CrearContexto();
            CargarUsuarioExistente(ctx);
            var controlador = CrearUsuarioController(ctx);

            var vm = NuevoRegistro();
            if (campo == nameof(vm.Legajo)) vm.Legajo = "0001";
            if (campo == nameof(vm.Dni)) vm.Dni = "44358717";
            if (campo == nameof(vm.Username)) vm.Username = "admin_1";

            var resultado = controlador.Registrar(vm);

            Assert.IsType<ViewResult>(resultado);
            Assert.True(controlador.ModelState.ContainsKey(campo));
            Assert.Single(ctx.Empleados);
        }

        [Fact]
        public void Usuario_Registrar_ConDatosNuevos_CreaEmpleadoYUsuario()
        {
            using var ctx = Prueba.CrearContexto();
            CargarUsuarioExistente(ctx);
            var controlador = CrearUsuarioController(ctx);

            var resultado = controlador.Registrar(NuevoRegistro());

            Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal(2, ctx.Empleados.Count());
            Assert.Equal(2, ctx.Usuarios.Count());
        }

        // =====================================================================
        // Pedido: el mozo siempre es el logueado y solo el admin lo reasigna
        // =====================================================================

        private static PedidoController CrearPedidoController(GastronomiaContext ctx, int idEmpleado, string rol)
        {
            var repoDetalle = new RepositorioDetallePedido(ctx);
            return Prueba.Preparar(new PedidoController(
                new RepositorioPedido(ctx, repoDetalle),
                repoDetalle,
                new RepositorioMesa(ctx),
                new RepositorioEmpleado(ctx),
                new RepositorioPlato(ctx),
                NullLogger<PedidoController>.Instance), idEmpleado, rol);
        }

        // Mozo 1 (activo) con un pedido abierto, mozo 2 activo y mozo 3 dado de baja.
        private static int CargarPedidoAbierto(GastronomiaContext ctx)
        {
            ctx.Empleados.AddRange(
                new Empleado { IdEmpleado = 1, Legajo = "0001", Nombre = "Ana", Apellido = "Uno", Dni = "30000001", Fecha_ingreso = DateTime.Today, Activo = true },
                new Empleado { IdEmpleado = 2, Legajo = "0002", Nombre = "Beto", Apellido = "Dos", Dni = "30000002", Fecha_ingreso = DateTime.Today, Activo = true },
                new Empleado { IdEmpleado = 3, Legajo = "0003", Nombre = "Caro", Apellido = "Tres", Dni = "30000003", Fecha_ingreso = DateTime.Today, Activo = false });
            ctx.Mesas.Add(new Mesa { IdMesa = 1, Numero = 1, Capacidad = 4, Tipo = "Mesa", Estado = true });
            var pedido = new Pedido { IdMesa = 1, IdEmpleado = 1, estado = Pedido.Estado.Abierto };
            ctx.Pedidos.Add(pedido);
            ctx.SaveChanges();
            ctx.ChangeTracker.Clear();
            return pedido.IdPedido;
        }

        [Fact]
        public void Pedido_Modificar_ComoMozo_NoPuedeCambiarElEmpleado()
        {
            using var ctx = Prueba.CrearContexto();
            int idPedido = CargarPedidoAbierto(ctx);
            var controlador = CrearPedidoController(ctx, idEmpleado: 1, rol: "2");

            controlador.Modificar(new Pedido { IdPedido = idPedido, IdMesa = 1, IdEmpleado = 2 });

            ctx.ChangeTracker.Clear();
            Assert.Equal(1, ctx.Pedidos.Single().IdEmpleado);
        }

        [Fact]
        public void Pedido_Modificar_ComoAdmin_PuedeReasignarAEmpleadoActivo()
        {
            using var ctx = Prueba.CrearContexto();
            int idPedido = CargarPedidoAbierto(ctx);
            var controlador = CrearPedidoController(ctx, idEmpleado: 1, rol: "1");

            controlador.Modificar(new Pedido { IdPedido = idPedido, IdMesa = 1, IdEmpleado = 2 });

            ctx.ChangeTracker.Clear();
            Assert.Equal(2, ctx.Pedidos.Single().IdEmpleado);
        }

        [Fact]
        public void Pedido_Modificar_ComoAdmin_NoPuedeAsignarEmpleadoDadoDeBaja()
        {
            using var ctx = Prueba.CrearContexto();
            int idPedido = CargarPedidoAbierto(ctx);
            var controlador = CrearPedidoController(ctx, idEmpleado: 1, rol: "1");

            var resultado = controlador.Modificar(new Pedido { IdPedido = idPedido, IdMesa = 1, IdEmpleado = 3 });

            var vista = Assert.IsType<ViewResult>(resultado);
            Assert.Equal("Editar", vista.ViewName);
            Assert.True(controlador.ModelState.ContainsKey(nameof(Pedido.IdEmpleado)));
        }

        [Fact]
        public void Pedido_Alta_SinEmpleadoEnLaSesion_RedirigeAlLogin()
        {
            using var ctx = Prueba.CrearContexto();
            CargarPedidoAbierto(ctx);
            var controlador = CrearPedidoController(ctx, idEmpleado: 0, rol: "2");

            var resultado = controlador.Alta(new Pedido { IdMesa = 1 });

            var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
            Assert.Equal("Login", redireccion.ControllerName);
        }
    }
}
