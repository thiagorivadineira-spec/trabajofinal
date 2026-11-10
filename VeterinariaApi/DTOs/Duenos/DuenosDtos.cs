using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class DuenoRequestDto
    {
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

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        // Opcional al dar de alta: crea la cuenta para la app móvil
        [MaxLength(50)]
        public string? Username { get; set; }

        public string? Password { get; set; }
    }

    public class CrearCuentaDuenoDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class DuenoResponseDto
    {
        public int Id { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public bool TieneCuenta { get; set; }
        public string? Username { get; set; }
        public int CantidadMascotas { get; set; }
    }
}
