using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    /// <summary>Datos profesionales de un usuario con rol Veterinario.</summary>
    public class Veterinario
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string Matricula { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }

        public int EspecialidadId { get; set; }
        public Especialidad Especialidad { get; set; } = null!;

        public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
        public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
    }
}
