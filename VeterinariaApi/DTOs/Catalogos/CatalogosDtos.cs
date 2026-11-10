using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    // ---------- Especialidades ----------
    public class EspecialidadRequestDto
    {
        [Required]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }

    public class EspecialidadResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int CantidadVeterinarios { get; set; }
    }

    // ---------- Veterinarios ----------
    public class VeterinarioResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public int EspecialidadId { get; set; }
        public string EspecialidadNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class VeterinarioUpdateDto
    {
        [Required]
        [MaxLength(30)]
        public string Matricula { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elegí una especialidad.")]
        public int EspecialidadId { get; set; }
    }

    // ---------- Distribuidores ----------
    public class DistribuidorRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Cuit { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }
    }

    public class DistribuidorResponseDto
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
