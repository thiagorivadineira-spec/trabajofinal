using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    /// <summary>Vacuna aplicada a una mascota.</summary>
    public class Vacuna
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaAplicacion { get; set; } = Services.Hora.Ahora;

        public DateTime? ProximaDosis { get; set; }

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;

        public int? AtencionId { get; set; }
        public Atencion? Atencion { get; set; }
    }
}
