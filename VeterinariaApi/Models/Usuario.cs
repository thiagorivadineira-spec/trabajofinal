using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.Models
{
    /// <summary>
    /// Cuenta de acceso al sistema. El rol decide qué puede hacer:
    /// Admin, Veterinario, Recepcionista (web) o Dueno (app móvil).
    /// </summary>
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        // Nombre para mostrar en pantalla
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Rol { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        // Datos extra según el rol (como máximo uno de los dos)
        public Veterinario? Veterinario { get; set; }
        public Dueno? Dueno { get; set; }
    }
}
