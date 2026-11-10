using System.ComponentModel.DataAnnotations;

namespace VeterinariaApi.DTOs
{
    public class CambiarPasswordRequestDto
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
