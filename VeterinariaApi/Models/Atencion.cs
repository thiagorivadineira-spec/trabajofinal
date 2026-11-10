using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinariaApi.Models
{
    /// <summary>Atención médica (consulta) registrada por un veterinario. Forma parte de la ficha clínica.</summary>
    public class Atencion
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = Services.Hora.Ahora;

        [MaxLength(200)]
        public string? Motivo { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Diagnostico { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Tratamiento { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // Peso medido en la consulta
        [Column(TypeName = "decimal(6,2)")]
        public decimal? Peso { get; set; }

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = null!;

        public int VeterinarioId { get; set; }
        public Veterinario Veterinario { get; set; } = null!;

        // Turno que originó la atención (puede no haber turno: urgencia)
        public int? TurnoId { get; set; }
        public Turno? Turno { get; set; }

        public ICollection<Receta> Recetas { get; set; } = new List<Receta>();
        public ICollection<Vacuna> Vacunas { get; set; } = new List<Vacuna>();
        public ICollection<Cirugia> Cirugias { get; set; } = new List<Cirugia>();
    }
}
