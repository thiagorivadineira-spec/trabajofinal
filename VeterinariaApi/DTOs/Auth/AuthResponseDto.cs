namespace VeterinariaApi.DTOs
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        // Solo uno tiene valor según el rol
        public int? VeterinarioId { get; set; }
        public int? DuenoId { get; set; }

        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }
}
