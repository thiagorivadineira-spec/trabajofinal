using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class EnviarReporteRequestDto
    {
        // "stock", "compras" o "atenciones"
        [Required]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Solo aplican a "compras" y "atenciones". Por defecto: últimos 30 días.
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }
}
