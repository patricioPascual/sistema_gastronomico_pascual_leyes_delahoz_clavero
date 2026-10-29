using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

namespace sistema_gastronomico.Tests.Infraestructura
{
    // Utilidades compartidas por todas las pruebas.
    public static class Prueba
    {
        // Ejecuta los atributos de validación del objeto, igual que hace MVC durante el model binding.
        // Devuelve la lista de errores (vacía si el objeto es válido).
        public static List<ValidationResult> Validar(object modelo)
        {
            var errores = new List<ValidationResult>();
            Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, validateAllProperties: true);
            return errores;
        }

        // true si alguno de los errores corresponde a la propiedad indicada.
        public static bool TieneErrorEn(this List<ValidationResult> errores, string propiedad)
        {
            return errores.Any(e => e.MemberNames.Contains(propiedad));
        }

        // Contexto de EF sobre una base en memoria nueva para cada prueba (nombre único),
        // así ninguna prueba ve los datos de otra.
        // La base en memoria no soporta transacciones: se ignora el aviso para que
        // BeginTransaction/Commit/Rollback de los controladores no tiren excepción.
        public static GastronomiaContext CrearContexto()
        {
            var opciones = new DbContextOptionsBuilder<GastronomiaContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new GastronomiaContext(opciones);
        }

        // Le da al controlador un HttpContext con un usuario logueado (claims como los que arma
        // ClaimsHelper) y TempData, que los controladores usan para los mensajes.
        public static T Preparar<T>(T controlador, int idEmpleado = 1, string rol = "1") where T : Controller
        {
            var identidad = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, rol),
                new Claim("IdEmpleado", idEmpleado.ToString()),
                new Claim("NombreCompleto", "Usuario Prueba")
            }, "Prueba");

            var http = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) };

            controlador.ControllerContext = new ControllerContext { HttpContext = http };
            controlador.TempData = new TempDataDictionary(http, new TempDataSinPersistencia());
            return controlador;
        }

        private class TempDataSinPersistencia : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }
    }
}
