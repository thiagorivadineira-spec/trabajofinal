using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    // ================= Registrar una atención =================
    public class AtencionRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Falta la mascota.")]
        public int MascotaId { get; set; }

        // Turno que se está atendiendo (null = atención sin turno / urgencia)
        public int? TurnoId { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        [MaxLength(1000)]
        public string Diagnostico { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Tratamiento { get; set; }

        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        [Range(0, 200)]
        public decimal? Peso { get; set; }

        public List<VacunaRequestDto> Vacunas { get; set; } = new();
        public List<AlergiaRequestDto> Alergias { get; set; } = new();
        public List<CirugiaRequestDto> Cirugias { get; set; } = new();
        public List<RecetaRequestDto> Recetas { get; set; } = new();
    }

    public class VacunaRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime? ProximaDosis { get; set; }
    }

    public class AlergiaRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CirugiaRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Tipo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Observaciones { get; set; }
    }

    public class RecetaRequestDto
    {
        public int MedicamentoId { get; set; }

        [Range(1, 1000)]
        public int Cantidad { get; set; } = 1;

        [MaxLength(100)]
        public string? Dosis { get; set; }

        [MaxLength(300)]
        public string? Indicaciones { get; set; }
    }

    // ================= Ficha clínica =================
    public class FichaClinicaDto
    {
        public MascotaResponseDto Mascota { get; set; } = new();
        public List<AlergiaResponseDto> Alergias { get; set; } = new();
        public List<VacunaResponseDto> Vacunas { get; set; } = new();
        public List<CirugiaResponseDto> Cirugias { get; set; } = new();
        public List<AtencionResponseDto> Atenciones { get; set; } = new();
    }

    public class AlergiaResponseDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }

    public class VacunaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaAplicacion { get; set; }
        public DateTime? ProximaDosis { get; set; }
    }

    public class CirugiaResponseDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Observaciones { get; set; }
    }

    public class AtencionResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string? Tratamiento { get; set; }
        public string? Observaciones { get; set; }
        public decimal? Peso { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
        public List<RecetaResponseDto> Recetas { get; set; } = new();
    }

    public class RecetaResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MascotaNombre { get; set; } = string.Empty;
        public int MedicamentoId { get; set; }
        public string MedicamentoNombre { get; set; } = string.Empty;
        public string? Presentacion { get; set; }
        public int Cantidad { get; set; }
        public string? Dosis { get; set; }
        public string? Indicaciones { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
    }
}
