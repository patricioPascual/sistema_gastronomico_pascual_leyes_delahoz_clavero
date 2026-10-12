namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public class AvatarViewModel
    {
        //Este ViewModel sirve para poder reutilizarlo tanto como para el layout y el perfil
        public string? AvatarPath { get; set; } //ruta de la imagen
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public int Tamanio { get; set; } = 40; //tamaño de foto 
    }
}
