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
    // Consulta: admin y veterinarios (para recetar). Alta/edición: solo admin.
    [Authorize(Roles = Roles.AdminOVeterinario)]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicamentosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public MedicamentosController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Medicamentos?buscar=amox&soloReponer=false&incluirInactivos=false
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentoResponseDto>>> GetMedicamentos(
            [FromQuery] string? buscar,
            [FromQuery] bool soloReponer = false,
            [FromQuery] bool incluirInactivos = false)
        {
            var query = _context.Medicamentos.AsNoTracking().AsQueryable();

            if (!incluirInactivos)
                query = query.Where(m => m.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termino = buscar.Trim().ToLower();
                query = query.Where(m => m.Nombre.ToLower().Contains(termino) ||
                                         (m.Presentacion != null && m.Presentacion.ToLower().Contains(termino)));
            }

            if (soloReponer)
                query = query.Where(m => m.StockActual <= m.StockMinimo);

            var lista = await query
                .OrderBy(m => m.Nombre)
                .Select(m => new MedicamentoResponseDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Presentacion = m.Presentacion,
                    Descripcion = m.Descripcion,
                    StockActual = m.StockActual,
                    StockMinimo = m.StockMinimo,
                    Activo = m.Activo,
                    Reponer = m.StockActual <= m.StockMinimo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/Medicamentos
        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Crear(MedicamentoRequestDto dto)
        {
            var medicamento = new Medicamento
            {
                Nombre = dto.Nombre.Trim(),
                Presentacion = dto.Presentacion,
                Descripcion = dto.Descripcion,
                StockMinimo = dto.StockMinimo,
                StockActual = dto.StockInicial,
                Activo = true
            };

            _context.Medicamentos.Add(medicamento);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("MEDICAMENTO_CREADO", "Medicamento", medicamento.Id,
                $"{medicamento.Nombre}; Stock inicial: {medicamento.StockActual}");

            return StatusCode(201, new { medicamento.Id });
        }

        // PUT: api/Medicamentos/5   (no modifica el stock: eso lo hacen compras y recetas)
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, MedicamentoRequestDto dto)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            medicamento.Nombre = dto.Nombre.Trim();
            medicamento.Presentacion = dto.Presentacion;
            medicamento.Descripcion = dto.Descripcion;
            medicamento.StockMinimo = dto.StockMinimo;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("MEDICAMENTO_EDITADO", "Medicamento", id, medicamento.Nombre);

            return NoContent();
        }

        // PUT: api/Medicamentos/5/activo?activo=false
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            var medicamento = await _context.Medicamentos.FindAsync(id);
            if (medicamento == null)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            medicamento.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
