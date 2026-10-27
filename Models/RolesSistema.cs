namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Constantes
{
    public static class RolesSistema
    {
        //ID de la BD si cambian en algun momemnto solo los debemos cambiar aca
        public const int AdministradorId = 1;
        public const int MozoId = 2;
        public const int CocineroId = 3;

        // Strings para los atributos [Authorize]
        public const string Administrador = "1";
        public const string Mozo = "2";
        public const string Cocinero = "3";

        // Combinaciones frecuentes para no concatenar en los controladores
        public const string AdminYMozo = Administrador + "," + Mozo;
        public const string Todos = Administrador + "," + Mozo + "," + Cocinero;
    }
}