namespace VeterinariaApi.DTOs
{
    /// <summary>Números para la pantalla de inicio de la web.</summary>
    public class ResumenDto
    {
        public int TurnosHoy { get; set; }
        public int TurnosPendientesHoy { get; set; }
        public int Mascotas { get; set; }
        public int Duenos { get; set; }
        public int Veterinarios { get; set; }
        public int MedicamentosAReponer { get; set; }
        public int AtencionesHoy { get; set; }
    }
}
