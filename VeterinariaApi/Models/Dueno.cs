using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    /// <summary>Dueño de una o más mascotas. Puede tener cuenta para usar la app móvil.</summary>
    public class Dueno
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Apellido { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;

        // Cuenta de acceso a la app (opcional)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}
