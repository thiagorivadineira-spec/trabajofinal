using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;
using VeterinariaApi.Models;
using VeterinariaApi.Seguridad;
using VeterinariaApi.Services;

namespace VeterinariaApi.Controllers
{
    // Recepción agenda, cancela y reprograma. El veterinario ve su agenda del día.
    [Authorize(Roles = Roles.Personal)]
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public TurnosController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Turnos?desde=2026-11-10&hasta=2026-11-10&veterinarioId=1&estado=Pendiente&mascotaId=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnoResponseDto>>> GetTurnos(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] int? veterinarioId,
            [FromQuery] int? mascotaId,
            [FromQuery] string? estado)
        {
            var desdeFecha = (desde ?? Hora.Hoy).Date;
            var hastaFecha = (hasta ?? desdeFecha).Date.AddDays(1);

            var query = _context.Turnos
                .AsNoTracking()
                .Where(t => t.FechaHora >= desdeFecha && t.FechaHora < hastaFecha);

            if (veterinarioId.HasValue)
                query = query.Where(t => t.VeterinarioId == veterinarioId.Value);

            if (mascotaId.HasValue)
                query = query.Where(t => t.MascotaId == mascotaId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(t => t.Estado == estado);

            return Ok(await Proyectar(query.OrderBy(t => t.FechaHora)).ToListAsync());
        }

        // GET: api/Turnos/agenda?fecha=2026-11-10   (turnos del veterinario que inició sesión)
        [Authorize(Roles = Roles.Veterinario)]
        [HttpGet("agenda")]
        public async Task<ActionResult<IEnumerable<TurnoResponseDto>>> GetAgenda([FromQuery] DateTime? fecha)
        {
            var veterinarioId = await VeterinarioActualAsync();
            if (veterinarioId == null)
                return BadRequest(new { mensaje = "Tu usuario no tiene ficha de veterinario." });

            var dia = (fecha ?? Hora.Hoy).Date;
            var diaSiguiente = dia.AddDays(1);

            var query = _context.Turnos
                .AsNoTracking()
                .Where(t => t.VeterinarioId == veterinarioId && t.FechaHora >= dia && t.FechaHora < diaSiguiente)
                .OrderBy(t => t.FechaHora);

            return Ok(await Proyectar(query).ToListAsync());
        }

        // POST: api/Turnos
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPost]
        public async Task<IActionResult> Crear(TurnoRequestDto dto)
        {
            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == dto.MascotaId && m.Activo);
            if (mascota == null)
                return BadRequest(new { mensaje = "La mascota indicada no existe." });

            var error = await ValidarHorarioAsync(dto.VeterinarioId, dto.FechaHora, null);
            if (error != null) return BadRequest(new { mensaje = error });

            var turno = new Turno
            {
                MascotaId = dto.MascotaId,
                VeterinarioId = dto.VeterinarioId,
                FechaHora = SinSegundos(dto.FechaHora),
                Motivo = dto.Motivo,
                Estado = Turno.Pendiente,
                FechaCreacion = Hora.Ahora
            };

            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("TURNO_CREADO", "Turno", turno.Id,
                $"{mascota.Nombre}; {turno.FechaHora:dd/MM/yyyy HH:mm}; Veterinario #{turno.VeterinarioId}");

            return StatusCode(201, new { turno.Id });
        }

        // PUT: api/Turnos/5/reprogramar
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPut("{id}/reprogramar")]
        public async Task<IActionResult> Reprogramar(int id, ReprogramarTurnoDto dto)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
                return NotFound(new { mensaje = "Turno no encontrado." });

            if (turno.Estado != Turno.Pendiente)
                return BadRequest(new { mensaje = "Solo se pueden reprogramar turnos pendientes." });

            var veterinarioId = dto.VeterinarioId ?? turno.VeterinarioId;

            var error = await ValidarHorarioAsync(veterinarioId, dto.FechaHora, id);
            if (error != null) return BadRequest(new { mensaje = error });

            var anterior = turno.FechaHora;
            turno.FechaHora = SinSegundos(dto.FechaHora);
            turno.VeterinarioId = veterinarioId;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("TURNO_REPROGRAMADO", "Turno", id,
                $"{anterior:dd/MM/yyyy HH:mm} -> {turno.FechaHora:dd/MM/yyyy HH:mm}");

            return NoContent();
        }

        // PUT: api/Turnos/5/cancelar
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
                return NotFound(new { mensaje = "Turno no encontrado." });

            if (turno.Estado != Turno.Pendiente)
                return BadRequest(new { mensaje = "Solo se pueden cancelar turnos pendientes." });

            turno.Estado = Turno.Cancelado;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("TURNO_CANCELADO", "Turno", id, $"{turno.FechaHora:dd/MM/yyyy HH:mm}");

            return NoContent();
        }

        // ----------------- auxiliares -----------------

        /// <summary>Un veterinario no puede tener dos turnos pendientes que se pisen (30 minutos cada uno).</summary>
        private async Task<string?> ValidarHorarioAsync(int veterinarioId, DateTime fechaHora, int? turnoIdExcluido)
        {
            if (!await _context.Veterinarios.AnyAsync(v => v.Id == veterinarioId && v.Usuario.Activo))
                return "El veterinario indicado no existe o está inactivo.";

            var inicio = SinSegundos(fechaHora);

            if (inicio < Hora.Ahora.AddMinutes(-5))
                return "No se puede dar un turno en una fecha u hora pasada.";

            var desde = inicio.AddMinutes(-Turno.DuracionMinutos);
            var hasta = inicio.AddMinutes(Turno.DuracionMinutos);

            var ocupado = await _context.Turnos.AnyAsync(t =>
                t.VeterinarioId == veterinarioId &&
                t.Estado == Turno.Pendiente &&
                t.Id != (turnoIdExcluido ?? 0) &&
                t.FechaHora > desde &&
                t.FechaHora < hasta);

            return ocupado
                ? $"El veterinario ya tiene un turno cerca de las {inicio:HH:mm}. Elegí otro horario."
                : null;
        }

        private async Task<int?> VeterinarioActualAsync()
        {
            var usuarioId = User.ObtenerUsuarioId();

            return await _context.Veterinarios
                .Where(v => v.UsuarioId == usuarioId)
                .Select(v => (int?)v.Id)
                .FirstOrDefaultAsync();
        }

        private static DateTime SinSegundos(DateTime f) =>
            new DateTime(f.Year, f.Month, f.Day, f.Hour, f.Minute, 0);

        private static IQueryable<TurnoResponseDto> Proyectar(IQueryable<Turno> query) =>
            query.Select(t => new TurnoResponseDto
            {
                Id = t.Id,
                FechaHora = t.FechaHora,
                Motivo = t.Motivo,
                Estado = t.Estado,
                MascotaId = t.MascotaId,
                MascotaNombre = t.Mascota.Nombre,
                Especie = t.Mascota.Especie,
                DuenoId = t.Mascota.DuenoId,
                DuenoNombre = t.Mascota.Dueno.Apellido + ", " + t.Mascota.Dueno.Nombre,
                DuenoTelefono = t.Mascota.Dueno.Telefono,
                VeterinarioId = t.VeterinarioId,
                VeterinarioNombre = t.Veterinario.Usuario.Nombre,
                EspecialidadNombre = t.Veterinario.Especialidad.Nombre
            });
    }
}
