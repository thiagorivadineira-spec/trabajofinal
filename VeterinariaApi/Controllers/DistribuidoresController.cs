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
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class DistribuidoresController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public DistribuidoresController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Distribuidores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DistribuidorResponseDto>>> GetDistribuidores([FromQuery] bool incluirInactivos = false)
        {
            var lista = await _context.Distribuidores
                .AsNoTracking()
                .Where(d => incluirInactivos || d.Activo)
                .OrderBy(d => d.RazonSocial)
                .Select(d => new DistribuidorResponseDto
                {
                    Id = d.Id,
                    RazonSocial = d.RazonSocial,
                    Cuit = d.Cuit,
                    Telefono = d.Telefono,
                    Email = d.Email,
                    Direccion = d.Direccion,
                    Activo = d.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/Distribuidores
        [HttpPost]
        public async Task<IActionResult> Crear(DistribuidorRequestDto dto)
        {
            var distribuidor = new Distribuidor
            {
                RazonSocial = dto.RazonSocial.Trim(),
                Cuit = dto.Cuit,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                Activo = true
            };

            _context.Distribuidores.Add(distribuidor);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("DISTRIBUIDOR_CREADO", "Distribuidor", distribuidor.Id, distribuidor.RazonSocial);

            return StatusCode(201, new { distribuidor.Id });
        }

        // PUT: api/Distribuidores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, DistribuidorRequestDto dto)
        {
            var distribuidor = await _context.Distribuidores.FindAsync(id);
            if (distribuidor == null)
                return NotFound(new { mensaje = "Distribuidor no encontrado." });

            distribuidor.RazonSocial = dto.RazonSocial.Trim();
            distribuidor.Cuit = dto.Cuit;
            distribuidor.Telefono = dto.Telefono;
            distribuidor.Email = dto.Email;
            distribuidor.Direccion = dto.Direccion;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("DISTRIBUIDOR_EDITADO", "Distribuidor", id, distribuidor.RazonSocial);

            return NoContent();
        }

        // PUT: api/Distribuidores/5/activo?activo=false
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            var distribuidor = await _context.Distribuidores.FindAsync(id);
            if (distribuidor == null)
                return NotFound(new { mensaje = "Distribuidor no encontrado." });

            distribuidor.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
