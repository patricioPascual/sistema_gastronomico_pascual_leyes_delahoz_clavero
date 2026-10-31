using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("MySql");

// GastronomiaContext con Pomelo para MySQL
builder.Services.AddDbContext<GastronomiaContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<RepositorioCompra>();
builder.Services.AddScoped<RepositorioProducto>();
builder.Services.AddScoped<RepositorioProveedor>();
builder.Services.AddScoped<RepositorioPedido>();
builder.Services.AddScoped<RepositorioDetallePedido>();
builder.Services.AddScoped<RepositorioMesa>();
builder.Services.AddScoped<RepositorioPlato>();
builder.Services.AddScoped<RepositorioDetalleReceta>();
builder.Services.AddScoped<RepositorioCategoria>();
builder.Services.AddScoped<RepositorioEmpleado>();
builder.Services.AddScoped<RepositorioRol>();
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioInforme>();

// Hasheo de contraseñas con el PasswordHasher
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Generación de tokens JWT para la API del mozo
builder.Services.AddScoped<ServicioJwt>();

// Autenticación híbrida: cookies para el sistema web, JWT para la API del mozo
builder.Services.AddAuthentication(options =>
{
    // Por defecto se usan cookies (sistema web MVC)
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login/Index";
    options.AccessDeniedPath = "/Login/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["TokenAuthentication:Issuer"],
        ValidAudience = builder.Configuration["TokenAuthentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(builder.Configuration["TokenAuthentication:SecretKey"] ?? string.Empty))
    };
});

// La autorización se resuelve con [Authorize(Roles = ...)] en cada controlador
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// MapStaticAssets (abajo) sirve wwwroot a partir de un manifest generado en tiempo de compilación,
// así que no alcanza a los avatares que los usuarios suben en runtime. UseStaticFiles cubre esos.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();