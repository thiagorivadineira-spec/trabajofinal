namespace VeterinariaWeb.Models
{
    // Clases que reflejan el JSON que devuelve / recibe la API (VeterinariaApi)

    // ---------------- Autenticación ----------------
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
        public int? VeterinarioId { get; set; }
        public int? DuenoId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }

    public class Paginado<T>
    {
        public int PaginaActual { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Datos { get; set; } = new();
    }

    public class IdRespuesta
    {
        public int Id { get; set; }
        public string? Mensaje { get; set; }
    }

    public class MensajeRespuesta
    {
        public string? Mensaje { get; set; }
    }

    // ---------------- Usuarios y personal ----------------
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public int? VeterinarioId { get; set; }
        public string? Matricula { get; set; }
        public string? EspecialidadNombre { get; set; }
    }

    public class UsuarioNuevo
    {
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Recepcionista";
        public string? Email { get; set; }
        public string? Matricula { get; set; }
        public int? EspecialidadId { get; set; }
        public string? Telefono { get; set; }
    }

    public class Especialidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int CantidadVeterinarios { get; set; }
    }

    public class EspecialidadRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class Veterinario
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

    public class Distribuidor
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }

    // ---------------- Dueños y mascotas ----------------
    public class Dueno
    {
        public int Id { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
        public bool TieneCuenta { get; set; }
        public string? Username { get; set; }
        public int CantidadMascotas { get; set; }
    }

    public class DuenoRequest
    {
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class CuentaRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
        public bool Activo { get; set; }
        public int DuenoId { get; set; }
        public string DuenoNombre { get; set; } = string.Empty;
        public string DuenoDni { get; set; } = string.Empty;
        public string? DuenoTelefono { get; set; }
    }

    public class MascotaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = "Perro";
        public string? Raza { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public int DuenoId { get; set; }
    }

    // ---------------- Turnos ----------------
    public class Turno
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

    public class TurnoRequest
    {
        public int MascotaId { get; set; }
        public int VeterinarioId { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Motivo { get; set; }
    }

    public class ReprogramarRequest
    {
        public DateTime FechaHora { get; set; }
        public int? VeterinarioId { get; set; }
    }

    // ---------------- Ficha clínica ----------------
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
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }

    public class Vacuna
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaAplicacion { get; set; }
        public DateTime? ProximaDosis { get; set; }
    }

    public class Cirugia
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Observaciones { get; set; }
    }

    public class Atencion
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string? Tratamiento { get; set; }
        public string? Observaciones { get; set; }
        public decimal? Peso { get; set; }
        public string VeterinarioNombre { get; set; } = string.Empty;
        public List<Receta> Recetas { get; set; } = new();
    }

    public class Receta
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

    public class AtencionRequest
    {
        public int MascotaId { get; set; }
        public int? TurnoId { get; set; }
        public string? Motivo { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string? Tratamiento { get; set; }
        public string? Observaciones { get; set; }
        public decimal? Peso { get; set; }
        public List<VacunaRequest> Vacunas { get; set; } = new();
        public List<AlergiaRequest> Alergias { get; set; } = new();
        public List<CirugiaRequest> Cirugias { get; set; } = new();
        public List<RecetaRequest> Recetas { get; set; } = new();
    }

    public class VacunaRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime? ProximaDosis { get; set; }
    }

    public class AlergiaRequest
    {
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CirugiaRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class RecetaRequest
    {
        public int MedicamentoId { get; set; }
        public int Cantidad { get; set; } = 1;
        public string? Dosis { get; set; }
        public string? Indicaciones { get; set; }
    }

    // ---------------- Medicamentos y compras ----------------
    public class Medicamento
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

    public class MedicamentoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Presentacion { get; set; }
        public string? Descripcion { get; set; }
        public int StockMinimo { get; set; }
        public int StockInicial { get; set; }
    }

    public class CompraRequest
    {
        public int DistribuidorId { get; set; }
        public List<DetalleCompraRequest> Detalles { get; set; } = new();
    }

    public class DetalleCompraRequest
    {
        public int MedicamentoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CompraCreada
    {
        public string? Mensaje { get; set; }
        public int CompraId { get; set; }
        public decimal Total { get; set; }
    }

    public class Compra
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string DistribuidorNombre { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public List<DetalleCompra> Detalles { get; set; } = new();
    }

    public class DetalleCompra
    {
        public int Id { get; set; }
        public string MedicamentoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    // ---------------- Inicio, auditoría, reportes, cuenta ----------------
    public class Resumen
    {
        public int TurnosHoy { get; set; }
        public int TurnosPendientesHoy { get; set; }
        public int Mascotas { get; set; }
        public int Duenos { get; set; }
        public int Veterinarios { get; set; }
        public int MedicamentosAReponer { get; set; }
        public int AtencionesHoy { get; set; }
    }

    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string? Username { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? DireccionIp { get; set; }
    }

    public class EnviarReporteRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }

    public class CambiarPasswordRequest
    {
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNueva { get; set; } = string.Empty;
    }

    // ---- Solo de pantalla (no viaja a la API) ----
    public class LineaCompra
    {
        public Medicamento Medicamento { get; set; } = null!;
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    /// <summary>Textos para mostrar cada rol.</summary>
    public static class RolTexto
    {
        public static string Mostrar(string? rol) => rol switch
        {
            "Admin" => "Administrador",
            "Veterinario" => "Veterinario",
            "Recepcionista" => "Recepcionista",
            "Dueno" => "Dueño",
            _ => rol ?? "-"
        };
    }
}
