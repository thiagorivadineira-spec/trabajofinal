using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;
using VeterinariaApi.Seguridad;
using VeterinariaApi.Services;

namespace VeterinariaApi.Controllers
{
    // Los veterinarios se crean desde api/Usuarios (rol Veterinario). Acá se consultan y editan.
    [Authorize(Roles = Roles.Personal)]
    [Route("api/[controller]")]
    [ApiController]
    public class VeterinariosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public VeterinariosController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Veterinarios?especialidadId=1   (recepción lo usa para asignar turnos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VeterinarioResponseDto>>> GetVeterinarios(
            [FromQuery] int? especialidadId,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.Veterinarios.AsNoTracking().AsQueryable();

            if (!incluirInactivos)
                query = query.Where(v => v.Usuario.Activo);

            if (especialidadId.HasValue)
                query = query.Where(v => v.EspecialidadId == especialidadId.Value);

            var lista = await query
                .OrderBy(v => v.Usuario.Nombre)
                .Select(v => new VeterinarioResponseDto
                {
                    Id = v.Id,
                    UsuarioId = v.UsuarioId,
                    Nombre = v.Usuario.Nombre,
                    Matricula = v.Matricula,
                    Telefono = v.Telefono,
                    EspecialidadId = v.EspecialidadId,
                    EspecialidadNombre = v.Especialidad.Nombre,
                    Activo = v.Usuario.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // PUT: api/Veterinarios/5
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, VeterinarioUpdateDto dto)
        {
            var veterinario = await _context.Veterinarios.FindAsync(id);
            if (veterinario == null)
                return NotFound(new { mensaje = "Veterinario no encontrado." });

            var matricula = dto.Matricula.Trim();

            if (await _context.Veterinarios.AnyAsync(v => v.Matricula == matricula && v.Id != id))
                return BadRequest(new { mensaje = "Ya existe otro veterinario con esa matrícula." });

            if (!await _context.Especialidades.AnyAsync(e => e.Id == dto.EspecialidadId))
                return BadRequest(new { mensaje = "La especialidad indicada no existe." });

            veterinario.Matricula = matricula;
            veterinario.Telefono = dto.Telefono;
            veterinario.EspecialidadId = dto.EspecialidadId;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("VETERINARIO_EDITADO", "Veterinario", id, $"Matrícula: {matricula}");

            return NoContent();
        }
    }
}
