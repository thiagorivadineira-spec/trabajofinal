namespace VeterinariaMovil.Models
{
    // Clases que reflejan el JSON de la API (solo lo que usa la app del dueño)

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? DuenoId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }

    public class Perfil
    {
        public int Id { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Username { get; set; }
        public int CantidadMascotas { get; set; }
    }

    public class Mascota
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }

        public string Emoji => Especie == "Gato" ? "🐱" : "🐶";
    }

    public class Turno
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Motivo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MascotaNombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string VeterinarioNombre { get; set; } = string.Empty;
        public string EspecialidadNombre { get; set; } = string.Empty;
    }

    public class FichaClinica
    {
        public Mascota Mascota { get; set; } = new();
        public List<Alergia> Alergias { get; set; } = new();
        public List<Vacuna> Vacunas { get; set; } = new();
        public List<Cirugia> Cirugias { get; set; } = new();
        public List<Atencion> Atenciones { get; set; } = new();
    }

    public class Alergia
    {
        public string Descripcion { get; set; } = string.Empty;
    }

    public class Vacuna
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaAplicacion { get; set; }
        public DateTime? ProximaDosis { get; set; }
    }

    public class Cirugia
    {
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Observaciones { get; set; }
    }

    public class Atencion
    {
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string? Tratamiento { get; set; }
        public decimal? Peso { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
        public List<Receta> Recetas { get; set; } = new();
    }

    public class Receta
    {
        public DateTime Fecha { get; set; }
        public string MascotaNombre { get; set; } = string.Empty;
        public string MedicamentoNombre { get; set; } = string.Empty;
        public string? Presentacion { get; set; }
        public int Cantidad { get; set; }
        public string? Dosis { get; set; }
        public string? Indicaciones { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }
}
