namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class RepositorioProveedor
    {
        private readonly GastronomiaContext _context;

        public RepositorioProveedor(GastronomiaContext context)
        {
            _context = context;
        }

        public int Alta(Proveedor proveedor)
        {
            proveedor.Estado = true;
            _context.Proveedores.Add(proveedor);
            _context.SaveChanges();
            return proveedor.IdProveedor;
        }

        public bool Modificar(Proveedor proveedor)
        {
            var proveedorExistente = _context.Proveedores
                .FirstOrDefault(p => p.IdProveedor == proveedor.IdProveedor && p.Estado);
            if (proveedorExistente == null)
                return false;

            proveedorExistente.Nombre = proveedor.Nombre;
            proveedorExistente.Cuit = proveedor.Cuit;
            proveedorExistente.Telefono = proveedor.Telefono;
            proveedorExistente.Direccion = proveedor.Direccion;
            return _context.SaveChanges() > 0;
        }

        public bool Baja(int idProveedor)
        {
            var proveedor = _context.Proveedores
                .FirstOrDefault(p => p.IdProveedor == idProveedor && p.Estado);
            if (proveedor == null)
                return false;

            proveedor.Estado = false;
            return _context.SaveChanges() > 0;
        }

        public Proveedor? ObtenerPorId(int idProveedor)
        {
            return _context.Proveedores
                .FirstOrDefault(p => p.IdProveedor == idProveedor && p.Estado);
        }

        public List<Proveedor> ObtenerTodos()
        {
            return _context.Proveedores
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .ToList();
        }
    }
}
