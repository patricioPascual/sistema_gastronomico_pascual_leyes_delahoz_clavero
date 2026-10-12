using System.Text;
using System.Security.Claims;
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

// Registramos el GastronomiaContext usando Pomelo para MySQL
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

// Preparado para cuando el mozo consuma la API/SPA
builder.Services.AddScoped<ServicioJwt>();

// Configuramos la Autenticación Hibrida (Cookies para el sistema web, JWT para la API del Mozo)
builder.Services.AddAuthentication(options =>
{
    // Mantenemos por defecto las cookies para que el sistema web MVC siga funcionando normal[cite: 20]
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
    // Esquema preparado para la API del mozo
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

// Crear Políticas de Autorización basadas en los Roles/Claims
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Administrador"));

    options.AddPolicy("PropietarioONomb", policy =>
        policy.RequireRole("Propietario", "Administrador"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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