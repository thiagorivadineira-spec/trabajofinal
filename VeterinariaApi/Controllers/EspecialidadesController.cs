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
    [Authorize(Roles = Roles.Personal)]
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public EspecialidadesController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Especialidades?incluirInactivas=false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspecialidadResponseDto>>> GetEspecialidades([FromQuery] bool incluirInactivas = false)
        {
            var lista = await _context.Especialidades
                .AsNoTracking()
                .Where(e => incluirInactivas || e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(e => new EspecialidadResponseDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Activo = e.Activo,
                    CantidadVeterinarios = e.Veterinarios.Count
                })
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/Especialidades
        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Crear(EspecialidadRequestDto dto)
        {
            var nombre = dto.Nombre.Trim();

            if (await _context.Especialidades.AnyAsync(e => e.Nombre == nombre))
                return BadRequest(new { mensaje = "Ya existe una especialidad con ese nombre." });

            var especialidad = new Especialidad { Nombre = nombre, Descripcion = dto.Descripcion, Activo = true };
            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("ESPECIALIDAD_CREADA", "Especialidad", especialidad.Id, nombre);

            return StatusCode(201, new { especialidad.Id });
        }

        // PUT: api/Especialidades/5
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, EspecialidadRequestDto dto)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad == null)
                return NotFound(new { mensaje = "Especialidad no encontrada." });

            especialidad.Nombre = dto.Nombre.Trim();
            especialidad.Descripcion = dto.Descripcion;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("ESPECIALIDAD_EDITADA", "Especialidad", id, especialidad.Nombre);

            return NoContent();
        }

        // PUT: api/Especialidades/5/activo?activo=false   (baja lógica)
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad == null)
                return NotFound(new { mensaje = "Especialidad no encontrada." });

            especialidad.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
