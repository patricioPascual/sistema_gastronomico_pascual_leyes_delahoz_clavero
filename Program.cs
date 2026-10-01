using Microsoft.EntityFrameworkCore;
using sistema_gastronomico_pascual_leyes_delahoz_clavero.Models;

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