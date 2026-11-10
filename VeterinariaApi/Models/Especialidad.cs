using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    public class Especialidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Veterinario> Veterinarios { get; set; } = new List<Veterinario>();
    }
}
