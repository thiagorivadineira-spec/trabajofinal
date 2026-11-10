using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;
using VeterinariaApi.Seguridad;
using VeterinariaApi.Services;

namespace VeterinariaApi.Controllers
{
    /// <summary>
    /// Endpoints de la app móvil para dueños. Todo se filtra por el dueño del token:
    /// un dueño nunca puede ver mascotas, turnos ni recetas de otro.
    /// </summary>
    [Authorize(Roles = Roles.Dueno)]
    [Route("api/[controller]")]
    [ApiController]
    public class PortalController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFichaClinicaService _fichas;

        public PortalController(DataContext context, IFichaClinicaService fichas)
        {
            _context = context;
            _fichas = fichas;
        }

        // GET: api/Portal/perfil
        [HttpGet("perfil")]
        public async Task<ActionResult<DuenoResponseDto>> GetPerfil()
        {
            var usuarioId = User.ObtenerUsuarioId();

            var perfil = await _context.Duenos
                .AsNoTracking()
                .Where(d => d.UsuarioId == usuarioId)
                .Select(d => new DuenoResponseDto
                {
                    Id = d.Id,
                    Dni = d.Dni,
                    Nombre = d.Nombre,
                    Apellido = d.Apellido,
                    NombreCompleto = d.Nombre + " " + d.Apellido,
                    Telefono = d.Telefono,
                    Email = d.Email,
                    Direccion = d.Direccion,
                    Activo = d.Activo,
                    TieneCuenta = true,
                    Username = d.Usuario != null ? d.Usuario.Username : null,
                    CantidadMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .FirstOrDefaultAsync();

            return perfil == null ? NotFound(new { mensaje = "Tu cuenta no está vinculada a un dueño." }) : Ok(perfil);
        }

        // GET: api/Portal/mascotas
        [HttpGet("mascotas")]
        public async Task<ActionResult<IEnumerable<MascotaResponseDto>>> GetMisMascotas()
        {
            var duenoId = await DuenoActualAsync();
            if (duenoId == null) return NotFound(new { mensaje = "Tu cuenta no está vinculada a un dueño." });

            var mascotas = await _context.Mascotas
                .AsNoTracking()
                .Where(m => m.DuenoId == duenoId && m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new MascotaResponseDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Especie = m.Especie,
                    Raza = m.Raza,
                    Sexo = m.Sexo,
                    FechaNacimiento = m.FechaNacimiento,
                    Peso = m.Peso,
                    Activo = m.Activo,
                    DuenoId = m.DuenoId,
                    DuenoNombre = m.Dueno.Nombre + " " + m.Dueno.Apellido,
                    DuenoDni = m.Dueno.Dni,
                    DuenoTelefono = m.Dueno.Telefono
                })
                .ToListAsync();

            return Ok(mascotas);
        }

        // GET: api/Portal/mascotas/5/historial
        [HttpGet("mascotas/{mascotaId}/historial")]
        public async Task<ActionResult<FichaClinicaDto>> GetHistorial(int mascotaId)
        {
            var duenoId = await DuenoActualAsync();

            var esSuya = duenoId != null &&
                         await _context.Mascotas.AnyAsync(m => m.Id == mascotaId && m.DuenoId == duenoId);

            // 404 y no 403: no se revela si la mascota existe
            if (!esSuya) return NotFound(new { mensaje = "Mascota no encontrada." });

            var ficha = await _fichas.ObtenerAsync(mascotaId);
            return ficha == null ? NotFound(new { mensaje = "Mascota no encontrada." }) : Ok(ficha);
        }

        // GET: api/Portal/turnos?incluirPasados=false
        [HttpGet("turnos")]
        public async Task<ActionResult<IEnumerable<TurnoResponseDto>>> GetMisTurnos([FromQuery] bool incluirPasados = false)
        {
            var duenoId = await DuenoActualAsync();
            if (duenoId == null) return NotFound(new { mensaje = "Tu cuenta no está vinculada a un dueño." });

            var hoy = Hora.Hoy;

            var turnos = await _context.Turnos
                .AsNoTracking()
                .Where(t => t.Mascota.DuenoId == duenoId && (incluirPasados || t.FechaHora >= hoy))
                .OrderBy(t => t.FechaHora)
                .Select(t => new TurnoResponseDto
                {
                    Id = t.Id,
                    FechaHora = t.FechaHora,
                    Motivo = t.Motivo,
                    Estado = t.Estado,
                    MascotaId = t.MascotaId,
                    MascotaNombre = t.Mascota.Nombre,
                    Especie = t.Mascota.Especie,
                    DuenoId = t.Mascota.DuenoId,
                    DuenoNombre = t.Mascota.Dueno.Nombre + " " + t.Mascota.Dueno.Apellido,
                    DuenoTelefono = t.Mascota.Dueno.Telefono,
                    VeterinarioId = t.VeterinarioId,
                    VeterinarioNombre = t.Veterinario.Usuario.Nombre,
                    EspecialidadNombre = t.Veterinario.Especialidad.Nombre
                })
                .ToListAsync();

            return Ok(turnos);
        }

        // GET: api/Portal/medicamentos   (todo lo recetado a sus mascotas, lo más nuevo primero)
        [HttpGet("medicamentos")]
        public async Task<ActionResult<IEnumerable<RecetaResponseDto>>> GetMedicamentosRecetados()
        {
            var duenoId = await DuenoActualAsync();
            if (duenoId == null) return NotFound(new { mensaje = "Tu cuenta no está vinculada a un dueño." });

            var recetas = await _context.Recetas
                .AsNoTracking()
                .Where(r => r.Atencion.Mascota.DuenoId == duenoId)
                .OrderByDescending(r => r.Atencion.Fecha)
                .Select(r => new RecetaResponseDto
                {
                    Id = r.Id,
                    Fecha = r.Atencion.Fecha,
                    MascotaNombre = r.Atencion.Mascota.Nombre,
                    MedicamentoId = r.MedicamentoId,
                    MedicamentoNombre = r.Medicamento.Nombre,
                    Presentacion = r.Medicamento.Presentacion,
                    Cantidad = r.Cantidad,
                    Dosis = r.Dosis,
                    Indicaciones = r.Indicaciones,
                    VeterinarioNombre = r.Atencion.Veterinario.Usuario.Nombre
                })
                .ToListAsync();

            return Ok(recetas);
        }

        private async Task<int?> DuenoActualAsync()
        {
            var usuarioId = User.ObtenerUsuarioId();

            return await _context.Duenos
                .Where(d => d.UsuarioId == usuarioId && d.Activo)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();
        }
    }
}
