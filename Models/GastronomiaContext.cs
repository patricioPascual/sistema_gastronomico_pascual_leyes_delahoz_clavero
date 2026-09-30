using Microsoft.EntityFrameworkCore;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class GastronomiaContext : DbContext
    {
        public GastronomiaContext(DbContextOptions<GastronomiaContext> options) : base(options)
        {
        }

        //Esto representaria cada tabla como una entidad 
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<DetalleReceta> DetalleRecetas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetalleCompras { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito de cada DbSet a su tabla correspondiente en MySQL
            modelBuilder.Entity<Empleado>().ToTable("empleado");
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Rol>().ToTable("rol");
            modelBuilder.Entity<Categoria>().ToTable("categoria");
            modelBuilder.Entity<Plato>().ToTable("plato");
            modelBuilder.Entity<DetalleReceta>().ToTable("detalle_receta");
            modelBuilder.Entity<Pedido>().ToTable("pedido");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");
            modelBuilder.Entity<Compra>().ToTable("compra");
            modelBuilder.Entity<DetalleCompra>().ToTable("detalle_compra");
            modelBuilder.Entity<Mesa>().ToTable("mesa");
            modelBuilder.Entity<Producto>().ToTable("producto");
            modelBuilder.Entity<Proveedor>().ToTable("proveedor");
        }
    }
}