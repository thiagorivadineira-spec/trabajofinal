namespace VeterinariaApi.DTOs
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Activo { get; set; }

        // Solo para veterinarios
        public int? VeterinarioId { get; set; }
        public string? Matricula { get; set; }
        public string? EspecialidadNombre { get; set; }
    }
}
