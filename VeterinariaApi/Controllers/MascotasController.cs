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
    // Buscar mascota: todo el personal. Alta y edición: recepción (y admin).
    // La ficha clínica completa está en AtencionesController (solo veterinarios y admin).
    [Authorize(Roles = Roles.Personal)]
    [Route("api/[controller]")]
    [ApiController]
    public class MascotasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public MascotasController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Mascotas?pagina=1&tamanoPagina=10&buscar=firulais&duenoId=3
        // "buscar" encuentra por nombre de la mascota, nombre/apellido o DNI del dueño
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<MascotaResponseDto>>> GetMascotas(
            [FromQuery] ParametrosPaginacionDto parametros,
            [FromQuery] int? duenoId,
            [FromQuery] bool incluirInactivas = false)
        {
            var query = _context.Mascotas.AsNoTracking().AsQueryable();

            if (!incluirInactivas)
                query = query.Where(m => m.Activo);

            if (duenoId.HasValue)
                query = query.Where(m => m.DuenoId == duenoId.Value);

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();
                query = query.Where(m =>
                    m.Nombre.ToLower().Contains(termino) ||
                    m.Dueno.Dni.Contains(termino) ||
                    m.Dueno.Nombre.ToLower().Contains(termino) ||
                    m.Dueno.Apellido.ToLower().Contains(termino));
            }

            var total = await query.CountAsync();

            var datos = await query
                .OrderBy(m => m.Nombre)
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
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
                    DuenoNombre = m.Dueno.Apellido + ", " + m.Dueno.Nombre,
                    DuenoDni = m.Dueno.Dni,
                    DuenoTelefono = m.Dueno.Telefono
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<MascotaResponseDto>(datos, total, parametros.Pagina, parametros.TamanoPagina));
        }

        // GET: api/Mascotas/5   (información básica del paciente)
        [HttpGet("{id}")]
        public async Task<ActionResult<MascotaResponseDto>> GetMascota(int id)
        {
            var mascota = await _context.Mascotas
                .AsNoTracking()
                .Where(m => m.Id == id)
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
                    DuenoNombre = m.Dueno.Apellido + ", " + m.Dueno.Nombre,
                    DuenoDni = m.Dueno.Dni,
                    DuenoTelefono = m.Dueno.Telefono
                })
                .FirstOrDefaultAsync();

            return mascota == null ? NotFound(new { mensaje = "Mascota no encontrada." }) : Ok(mascota);
        }

        // POST: api/Mascotas
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPost]
        public async Task<IActionResult> Crear(MascotaRequestDto dto)
        {
            var error = await ValidarAsync(dto);
            if (error != null) return BadRequest(new { mensaje = error });

            var mascota = new Mascota
            {
                Nombre = dto.Nombre.Trim(),
                Especie = NormalizarEspecie(dto.Especie),
                Raza = dto.Raza,
                Sexo = dto.Sexo,
                FechaNacimiento = dto.FechaNacimiento,
                Peso = dto.Peso,
                DuenoId = dto.DuenoId,
                Activo = true
            };

            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("MASCOTA_CREADA", "Mascota", mascota.Id,
                $"{mascota.Nombre} ({mascota.Especie}); Dueño #{mascota.DuenoId}");

            return StatusCode(201, new { mascota.Id });
        }

        // PUT: api/Mascotas/5
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, MascotaRequestDto dto)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null)
                return NotFound(new { mensaje = "Mascota no encontrada." });

            var error = await ValidarAsync(dto);
            if (error != null) return BadRequest(new { mensaje = error });

            mascota.Nombre = dto.Nombre.Trim();
            mascota.Especie = NormalizarEspecie(dto.Especie);
            mascota.Raza = dto.Raza;
            mascota.Sexo = dto.Sexo;
            mascota.FechaNacimiento = dto.FechaNacimiento;
            mascota.Peso = dto.Peso;
            mascota.DuenoId = dto.DuenoId;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("MASCOTA_EDITADA", "Mascota", id, mascota.Nombre);

            return NoContent();
        }

        // PUT: api/Mascotas/5/activo?activo=false
        [Authorize(Roles = Roles.AdminORecepcion)]
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null)
                return NotFound(new { mensaje = "Mascota no encontrada." });

            mascota.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string?> ValidarAsync(MascotaRequestDto dto)
        {
            var especie = NormalizarEspecie(dto.Especie);
            if (especie != Mascota.Perro && especie != Mascota.Gato)
                return "La veterinaria solo atiende perros y gatos.";

            if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento.Value.Date > Hora.Hoy)
                return "La fecha de nacimiento no puede ser futura.";

            if (!await _context.Duenos.AnyAsync(d => d.Id == dto.DuenoId && d.Activo))
                return "El dueño indicado no existe.";

            return null;
        }

        private static string NormalizarEspecie(string? especie)
        {
            var e = (especie ?? string.Empty).Trim().ToLowerInvariant();
            return e switch
            {
                "perro" => Mascota.Perro,
                "gato" => Mascota.Gato,
                _ => e
            };
        }
    }
}
