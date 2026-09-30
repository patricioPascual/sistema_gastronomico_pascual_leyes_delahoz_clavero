using Microsoft.EntityFrameworkCore;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

var builder = WebApplication.CreateBuilder(args);

// Ahora nuestras vistas utilizan los controller sin los repositorios
builder.Services.AddControllersWithViews();

// 1. Obtenemos la cadena de conexión desde el appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MySql");

// 2. Registramos el GastronomiaContext usando Pomelo para MySQL
builder.Services.AddDbContext<GastronomiaContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Nota: Ya no necesitamos registrar los Repositorios antiguos (RepositorioPlato, RepositorioEmpleado, etc.) 
// porque ahora los controladores trabajarán directamente con el GastronomiaContext de Entity Framework Core.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Plato}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();