using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class TurnoRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Elegí la mascota.")]
        public int MascotaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elegí el veterinario.")]
        public int VeterinarioId { get; set; }

        public DateTime FechaHora { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }
    }

    public class ReprogramarTurnoDto
    {
        public DateTime FechaHora { get; set; }

        // Opcional: cambiar también de veterinario
        public int? VeterinarioId { get; set; }
    }

    public class TurnoResponseDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Motivo { get; set; }
        public string Estado { get; set; } = string.Empty;

        public int MascotaId { get; set; }
        public string MascotaNombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;

        public int DuenoId { get; set; }
        public string DuenoNombre { get; set; } = string.Empty;
        public string? DuenoTelefono { get; set; }

        public int VeterinarioId { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
        public string EspecialidadNombre { get; set; } = string.Empty;
    }
}
