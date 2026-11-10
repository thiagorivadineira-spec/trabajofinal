using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    public class Alergia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = Services.Hora.Ahora;

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;
    }
}
