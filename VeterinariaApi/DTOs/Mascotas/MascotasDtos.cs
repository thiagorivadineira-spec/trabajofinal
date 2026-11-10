using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class MascotaRequestDto
    {
        [Required]
        [MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        // "Perro" o "Gato"
        [Required]
        public string Especie { get; set; } = "Perro";

        [MaxLength(60)]
        public string? Raza { get; set; }

        // "Macho" o "Hembra"
        [MaxLength(10)]
        public string? Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Range(0, 200)]
        public decimal? Peso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elegí el dueño de la mascota.")]
        public int DuenoId { get; set; }
    }

    public class MascotaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public bool Activo { get; set; }

        public int DuenoId { get; set; }
        public string DuenoNombre { get; set; } = string.Empty;
        public string DuenoDni { get; set; } = string.Empty;
        public string? DuenoTelefono { get; set; }
    }
}
