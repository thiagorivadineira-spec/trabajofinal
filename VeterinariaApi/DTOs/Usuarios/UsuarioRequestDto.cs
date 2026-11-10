using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    /// <summary>Alta de personal (Admin, Veterinario o Recepcionista) por parte de un administrador.</summary>
    public class UsuarioRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        // Obligatorios solo si Rol = Veterinario
        [MaxLength(30)]
        public string? Matricula { get; set; }

        public int? EspecialidadId { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }
    }
}

namespace VeterinariaApi.DTOs
{
    public class PasswordRequestDto
    {
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
