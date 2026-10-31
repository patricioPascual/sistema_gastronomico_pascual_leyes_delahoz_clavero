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


        // activos = true lista los activos (por defecto); false lista los dados de baja
        public List<Proveedor> ObtenerLista(int pagNro, int tamPagina, bool activos = true)
        {
            int offset = (pagNro - 1) * tamPagina;
            return _context.Proveedores
                .Where(p => p.Estado == activos)
                .OrderBy(p => p.Nombre)
                .Skip(offset)
                .Take(tamPagina)
                .ToList();
        }

        public int ObtenerCantidad(bool activos = true)
        {
            return _context.Proveedores.Count(p => p.Estado == activos);
        }

        public Proveedor? ObtenerInactivoPorId(int idProveedor)
        {
            return _context.Proveedores
                .FirstOrDefault(p => p.IdProveedor == idProveedor && !p.Estado);
        }

        public bool Reactivar(int idProveedor)
        {
            var proveedor = ObtenerInactivoPorId(idProveedor);
            if (proveedor == null)
                return false;

            proveedor.Estado = true;
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

        // Busqueda para el Select2 AJAX: solo activos maximo 20 resultados.
        public List<Proveedor> Buscar(string q)
        {
            return _context.Proveedores
                .Where(p => p.Estado && p.Nombre != null && p.Nombre.Contains(q))
                .OrderBy(p => p.Nombre)
                .Take(20)
                .ToList();
        }

        // CUIT repetido entre proveedores activos (la columna no tiene UNIQUE en la base).
        public bool ExisteCuitActivo(string cuit, int excluirId = 0)
        {
            return _context.Proveedores.Any(p => p.Estado && p.Cuit == cuit && p.IdProveedor != excluirId);
        }
    }
}
