using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    public class Turno
    {
        public const string Pendiente = "Pendiente";
        public const string Atendido = "Atendido";
        public const string Cancelado = "Cancelado";

        // Duración fija de cada turno (se usa para evitar superposiciones)
        public const int DuracionMinutos = 30;

        [Key]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = Pendiente;

        public DateTime FechaCreacion { get; set; } = Services.Hora.Ahora;

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;

        public int VeterinarioId { get; set; }
        public Veterinario Veterinario { get; set; } = null!;
    }
}
