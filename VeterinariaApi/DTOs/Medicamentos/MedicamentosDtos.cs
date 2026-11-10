using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class MedicamentoRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Presentacion { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Range(0, 100000)]
        public int StockMinimo { get; set; }

        // Solo se usa al crear. Después el stock cambia por compras (suma) y recetas (resta).
        [Range(0, 100000)]
        public int StockInicial { get; set; }
    }

    public class MedicamentoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Presentacion { get; set; }
        public string? Descripcion { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }
        public bool Reponer { get; set; }
    }
}
