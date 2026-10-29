using sistema_gastronomico.Tests.Infraestructura;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Helpers;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using Xunit;

namespace sistema_gastronomico.Tests
{
    // Pruebas unitarias de los atributos de validación de los modelos.
    // No usan base de datos ni controladores: arman un objeto válido, le rompen UN campo
    // y verifican que el error aparezca justo en ese campo (patrón Arrange / Act / Assert).
    public class ValidacionModelosTests
    {
        // ---------- Objetos válidos de partida (mismos datos que el seed) ----------

        private static RegistrarUsuarioViewModel RegistroValido() => new()
        {
            Legajo = "0001",
            Nombre = "Leandro",
            Apellido = "Leyes",
            Dni = "44358717",
            Telefono = "2664123456",
            FechaIngreso = DateTime.Today,
            IdRol = 1,
            Username = "admin_1",
            Password = "clave123",
            ConfirmPassword = "clave123"
        };

        private static EditarUsuarioViewModel EdicionValida() => new()
        {
            IdUsuario = 1,
            IdEmpleado = 1,
            Legajo = "0001",
            Nombre = "Leandro",
            Apellido = "Leyes",
            Dni = "44358717",
            IdRol = 1,
            Username = "admin_1"
        };

        private static Empleado EmpleadoValido() => new()
        {
            Legajo = "0001",
            Nombre = "Leandro",
            Apellido = "Leyes",
            Dni = "44358717",
            Fecha_ingreso = new DateTime(2026, 10, 12),
            Activo = true
        };

        private static Producto ProductoValido() => new()
        {
            Nombre = "Harina 000",
            Cantidad_stock = 25,
            Unidad_medida = "kg",
            Precio_costo = 950,
            Estado = true
        };

        private static Proveedor ProveedorValido() => new()
        {
            Nombre = "Verdulería La Quinta",
            Cuit = "30-71111111-1",
            Telefono = "2665407728",
            Direccion = "Los Lapachos 123"
        };

        private static Compra CompraValida() => new()
        {
            IdProveedor = 1,
            IdEmpleado = 1,
            FechaHora = DateTime.Now,
            NumeroComprobante = "FC-0001-1234"
        };

        private static Plato PlatoValido() => new()
        {
            Nombre = "Milanesa con papas",
            PrecioVenta = 9500,
            IdCategoria = 1,
            Estado = true
        };

        // ---------- Objetos de partida ----------

        [Fact]
        public void LosObjetosDePartidaSonValidos()
        {
            Assert.Empty(Prueba.Validar(RegistroValido()));
            Assert.Empty(Prueba.Validar(EdicionValida()));
            Assert.Empty(Prueba.Validar(EmpleadoValido()));
            Assert.Empty(Prueba.Validar(ProductoValido()));
            Assert.Empty(Prueba.Validar(ProveedorValido()));
            Assert.Empty(Prueba.Validar(CompraValida()));
            Assert.Empty(Prueba.Validar(PlatoValido()));
        }

        // ---------- DNI ----------

        [Theory]
        [InlineData("1234567")]   // 7 dígitos
        [InlineData("44358717")]  // 8 dígitos
        public void Dni_Valido(string dni)
        {
            var vm = RegistroValido();
            vm.Dni = dni;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Dni)));
        }

        [Theory]
        [InlineData("123456")]      // corto
        [InlineData("123456789")]   // largo
        [InlineData("44A58717")]    // letras
        [InlineData("44.358.717")]  // con puntos
        [InlineData("44 358 717")]  // con espacios
        public void Dni_Invalido(string dni)
        {
            var vm = RegistroValido();
            vm.Dni = dni;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Dni)));
        }

        // ---------- Legajo ----------

        [Theory]
        [InlineData("0001")]
        [InlineData("1234567890")]
        public void Legajo_Valido(string legajo)
        {
            var vm = RegistroValido();
            vm.Legajo = legajo;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Legajo)));
        }

        [Theory]
        [InlineData("001")]          // menos de 4
        [InlineData("12345678901")]  // más de 10
        [InlineData("EMP-01")]       // letras
        public void Legajo_Invalido(string legajo)
        {
            var vm = RegistroValido();
            vm.Legajo = legajo;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Legajo)));
        }

        // ---------- Nombre y apellido ----------

        [Theory]
        [InlineData("María José")]
        [InlineData("D'Angelo")]
        [InlineData("Pérez-Gil")]
        [InlineData("Ñandú")]
        public void Nombre_Valido(string nombre)
        {
            var vm = RegistroValido();
            vm.Nombre = nombre;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Nombre)));
        }

        [Theory]
        [InlineData("Juan2")]
        [InlineData("Juan@")]
        [InlineData("J")]  // menos de 2 caracteres
        public void Nombre_Invalido(string nombre)
        {
            var vm = RegistroValido();
            vm.Nombre = nombre;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Nombre)));
        }

        [Fact]
        public void Apellido_DeMasDe100Caracteres_EsInvalido()
        {
            var vm = RegistroValido();
            vm.Apellido = new string('a', 101);
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Apellido)));
        }

        // ---------- Teléfono del empleado ----------

        [Theory]
        [InlineData(null)]  // es opcional
        [InlineData("2664123456")]
        [InlineData("+54 266 4123456")]
        [InlineData("(266) 412-3456")]
        public void Telefono_Valido(string? telefono)
        {
            var vm = RegistroValido();
            vm.Telefono = telefono;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Telefono)));
        }

        [Theory]
        [InlineData("abc123")]
        [InlineData("12345")]  // menos de 6
        public void Telefono_Invalido(string telefono)
        {
            var vm = RegistroValido();
            vm.Telefono = telefono;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Telefono)));
        }

        // ---------- Fecha de ingreso ----------

        [Fact]
        public void FechaIngreso_Hoy_EsValida()
        {
            var vm = RegistroValido();
            vm.FechaIngreso = DateTime.Today;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.FechaIngreso)));
        }

        [Fact]
        public void FechaIngreso_Futura_EsInvalida()
        {
            var vm = RegistroValido();
            vm.FechaIngreso = DateTime.Today.AddDays(1);
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.FechaIngreso)));
        }

        [Fact]
        public void FechaIngreso_Anio1900_EsInvalida()
        {
            var vm = RegistroValido();
            vm.FechaIngreso = new DateTime(1900, 1, 1);
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.FechaIngreso)));
        }

        [Fact]
        public void Empleado_FechaIngresoFutura_EsInvalida()
        {
            var empleado = EmpleadoValido();
            empleado.Fecha_ingreso = DateTime.Today.AddDays(1);
            Assert.True(Prueba.Validar(empleado).TieneErrorEn(nameof(empleado.Fecha_ingreso)));
        }

        // ---------- Username ----------

        [Theory]
        [InlineData("admin_1")]
        [InlineData("juan.perez")]
        public void Username_Valido(string username)
        {
            var vm = RegistroValido();
            vm.Username = username;
            Assert.False(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Username)));
        }

        [Theory]
        [InlineData("abc")]          // menos de 4
        [InlineData("juan perez")]   // espacio
        [InlineData("juan#perez")]   // símbolo
        [InlineData("josé")]         // tilde
        public void Username_Invalido(string username)
        {
            var vm = RegistroValido();
            vm.Username = username;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Username)));
        }

        // ---------- Contraseña (alta) ----------

        [Theory]
        [InlineData("ab12")]        // corta
        [InlineData("abcdefgh")]    // sin número
        [InlineData("12345678")]    // sin letra
        [InlineData("abc 12345")]   // con espacio
        public void Password_Invalida(string password)
        {
            var vm = RegistroValido();
            vm.Password = password;
            vm.ConfirmPassword = password;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Password)));
        }

        [Fact]
        public void Password_DeMasDe64Caracteres_EsInvalida()
        {
            var vm = RegistroValido();
            vm.Password = "a1" + new string('x', 63);
            vm.ConfirmPassword = vm.Password;
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.Password)));
        }

        [Fact]
        public void Password_QueNoCoincide_EsInvalida()
        {
            var vm = RegistroValido();
            vm.ConfirmPassword = "otra1234";
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.ConfirmPassword)));
        }

        // ---------- Contraseña (edición: es opcional) ----------

        [Fact]
        public void Edicion_SinNuevaPassword_EsValida()
        {
            var vm = EdicionValida();
            vm.NuevaPassword = null;
            vm.ConfirmarNuevaPassword = null;
            Assert.Empty(Prueba.Validar(vm));
        }

        [Fact]
        public void Edicion_NuevaPasswordDebil_EsInvalida()
        {
            var vm = EdicionValida();
            vm.NuevaPassword = "123";
            vm.ConfirmarNuevaPassword = "123";
            Assert.True(Prueba.Validar(vm).TieneErrorEn(nameof(vm.NuevaPassword)));
        }

        // ---------- Producto: unidad de medida y largo del nombre ----------

        [Fact]
        public void Producto_TodasLasUnidadesDeLaLista_SonValidas()
        {
            foreach (var unidad in UnidadesMedida.Todas.Keys)
            {
                var producto = ProductoValido();
                producto.Unidad_medida = unidad;
                Assert.False(Prueba.Validar(producto).TieneErrorEn(nameof(producto.Unidad_medida)), $"Falló con '{unidad}'");
            }
        }

        [Theory]
        [InlineData("Kilos")]
        [InlineData("kgs")]
        [InlineData("litro")]  // la que usaba el seed viejo
        [InlineData("KG")]     // la lista distingue mayúsculas
        public void Producto_UnidadFueraDeLaLista_EsInvalida(string unidad)
        {
            var producto = ProductoValido();
            producto.Unidad_medida = unidad;
            Assert.True(Prueba.Validar(producto).TieneErrorEn(nameof(producto.Unidad_medida)));
        }

        [Fact]
        public void Producto_NombreDeMasDe100Caracteres_EsInvalido()
        {
            var producto = ProductoValido();
            producto.Nombre = new string('a', 101);
            Assert.True(Prueba.Validar(producto).TieneErrorEn(nameof(producto.Nombre)));
        }

        // ---------- Plato ----------

        [Fact]
        public void Plato_NombreDeMasDe120Caracteres_EsInvalido()
        {
            var plato = PlatoValido();
            plato.Nombre = new string('a', 121);
            Assert.True(Prueba.Validar(plato).TieneErrorEn(nameof(plato.Nombre)));
        }

        [Fact]
        public void Plato_SinCategoria_EsInvalido()
        {
            var plato = PlatoValido();
            plato.IdCategoria = 0;
            Assert.True(Prueba.Validar(plato).TieneErrorEn(nameof(plato.IdCategoria)));
        }

        // ---------- Proveedor: CUIT ----------

        [Theory]
        [InlineData("30-71111111-1")]
        [InlineData("30-70000000-8")]
        [InlineData("20-12345678-6")]
        public void Cuit_ConDigitoVerificadorCorrecto_EsValido(string cuit)
        {
            Assert.True(CuitValidoAttribute.EsCuitValido(cuit));

            var proveedor = ProveedorValido();
            proveedor.Cuit = cuit;
            Assert.False(Prueba.Validar(proveedor).TieneErrorEn(nameof(proveedor.Cuit)));
        }

        [Theory]
        [InlineData("30-72222222-2")]  // dígito incorrecto (seed viejo)
        [InlineData("30-76666666-9")]  // el cálculo da 10: no existe CUIT válido con esa base
        public void Cuit_ConDigitoVerificadorIncorrecto_EsInvalido(string cuit)
        {
            Assert.False(CuitValidoAttribute.EsCuitValido(cuit));

            var proveedor = ProveedorValido();
            proveedor.Cuit = cuit;
            Assert.True(Prueba.Validar(proveedor).TieneErrorEn(nameof(proveedor.Cuit)));
        }

        [Theory]
        [InlineData("30711111111")]    // sin guiones
        [InlineData("30-7111111-1")]   // faltan dígitos
        public void Cuit_ConFormatoIncorrecto_EsInvalido(string cuit)
        {
            var proveedor = ProveedorValido();
            proveedor.Cuit = cuit;

            var errores = Prueba.Validar(proveedor).Where(e => e.MemberNames.Contains(nameof(proveedor.Cuit))).ToList();

            // Un solo mensaje: el de formato. El de dígito verificador no se suma.
            Assert.Single(errores);
        }

        [Fact]
        public void Proveedor_DireccionDeMasDe150Caracteres_EsInvalida()
        {
            var proveedor = ProveedorValido();
            proveedor.Direccion = new string('a', 151);
            Assert.True(Prueba.Validar(proveedor).TieneErrorEn(nameof(proveedor.Direccion)));
        }

        // ---------- Compra ----------

        [Theory]
        [InlineData(null)]  // es opcional
        [InlineData("FC-0001-1234")]
        [InlineData("A-00002-00012345")]
        [InlineData("0001-123")]
        public void Comprobante_Valido(string? numero)
        {
            var compra = CompraValida();
            compra.NumeroComprobante = numero;
            Assert.False(Prueba.Validar(compra).TieneErrorEn(nameof(compra.NumeroComprobante)));
        }

        [Theory]
        [InlineData("factura 12")]
        [InlineData("FC-01-1234")]          // punto de venta corto
        [InlineData("FC-0001-123456789")]   // número de más de 8 dígitos
        public void Comprobante_Invalido(string numero)
        {
            var compra = CompraValida();
            compra.NumeroComprobante = numero;
            Assert.True(Prueba.Validar(compra).TieneErrorEn(nameof(compra.NumeroComprobante)));
        }

        [Fact]
        public void Compra_FechaDeManiana_EsInvalida()
        {
            var compra = CompraValida();
            compra.FechaHora = DateTime.Now.AddDays(1);
            Assert.True(Prueba.Validar(compra).TieneErrorEn(nameof(compra.FechaHora)));
        }

        [Fact]
        public void Compra_FechaDentroDeLaTolerancia_EsValida()
        {
            var compra = CompraValida();
            compra.FechaHora = DateTime.Now.AddMinutes(3);
            Assert.False(Prueba.Validar(compra).TieneErrorEn(nameof(compra.FechaHora)));
        }

        [Fact]
        public void Compra_FechaAnteriorAl2000_EsInvalida()
        {
            var compra = CompraValida();
            compra.FechaHora = new DateTime(1999, 12, 31);
            Assert.True(Prueba.Validar(compra).TieneErrorEn(nameof(compra.FechaHora)));
        }
    }
}
