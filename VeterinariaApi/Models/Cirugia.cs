using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    public class Cirugia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Tipo { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = Services.Hora.Ahora;

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;

        public int? AtencionId { get; set; }
        public Atencion? Atencion { get; set; }
    }
}
