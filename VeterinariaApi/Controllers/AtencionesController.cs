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
    // Ficha clínica y atención médica: veterinarios (el admin puede consultar la ficha)
    [Authorize(Roles = Roles.AdminOVeterinario)]
    [Route("api/[controller]")]
    [ApiController]
    public class AtencionesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFichaClinicaService _fichas;
        private readonly IAuditoriaService _auditoria;

        public AtencionesController(DataContext context, IFichaClinicaService fichas, IAuditoriaService auditoria)
        {
            _context = context;
            _fichas = fichas;
            _auditoria = auditoria;
        }

        // GET: api/Atenciones/ficha/5   (ficha clínica completa de la mascota 5)
        [HttpGet("ficha/{mascotaId}")]
        public async Task<ActionResult<FichaClinicaDto>> GetFicha(int mascotaId)
        {
            var ficha = await _fichas.ObtenerAsync(mascotaId);

            return ficha == null ? NotFound(new { mensaje = "Mascota no encontrada." }) : Ok(ficha);
        }

        // POST: api/Atenciones
        // Registra en una sola operación: atención + diagnóstico + tratamiento + vacunas +
        // alergias + cirugías + medicamentos recetados (que descuentan stock). Si vino de un turno, lo marca Atendido.
        [Authorize(Roles = Roles.Veterinario)]
        [HttpPost]
        public async Task<IActionResult> Registrar(AtencionRequestDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();

            var veterinario = await _context.Veterinarios.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
            if (veterinario == null)
                return BadRequest(new { mensaje = "Tu usuario no tiene ficha de veterinario." });

            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == dto.MascotaId);
            if (mascota == null)
                return BadRequest(new { mensaje = "La mascota indicada no existe." });

            Turno? turno = null;

            if (dto.TurnoId.HasValue)
            {
                turno = await _context.Turnos.FirstOrDefaultAsync(t => t.Id == dto.TurnoId.Value);

                if (turno == null || turno.MascotaId != mascota.Id)
                    return BadRequest(new { mensaje = "El turno no corresponde a esta mascota." });

                if (turno.Estado != Turno.Pendiente)
                    return BadRequest(new { mensaje = $"El turno ya está {turno.Estado.ToLower()}." });
            }

            // Validar recetas y stock antes de tocar nada
            var medicamentos = new Dictionary<int, Medicamento>();

            foreach (var grupo in dto.Recetas.GroupBy(r => r.MedicamentoId))
            {
                var medicamento = await _context.Medicamentos.FirstOrDefaultAsync(m => m.Id == grupo.Key && m.Activo);
                if (medicamento == null)
                    return BadRequest(new { mensaje = $"El medicamento #{grupo.Key} no existe." });

                var pedido = grupo.Sum(r => r.Cantidad);
                if (pedido <= 0)
                    return BadRequest(new { mensaje = "La cantidad recetada debe ser mayor a cero." });

                if (medicamento.StockActual < pedido)
                    return BadRequest(new { mensaje = $"Stock insuficiente de {medicamento.Nombre}: hay {medicamento.StockActual}, se pidieron {pedido}." });

                medicamentos[medicamento.Id] = medicamento;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ahora = Hora.Ahora;

                var atencion = new Atencion
                {
                    Fecha = ahora,
                    MascotaId = mascota.Id,
                    VeterinarioId = veterinario.Id,
                    TurnoId = turno?.Id,
                    Motivo = dto.Motivo ?? turno?.Motivo,
                    Diagnostico = dto.Diagnostico.Trim(),
                    Tratamiento = dto.Tratamiento,
                    Observaciones = dto.Observaciones,
                    Peso = dto.Peso
                };

                foreach (var v in dto.Vacunas.Where(v => !string.IsNullOrWhiteSpace(v.Nombre)))
                {
                    atencion.Vacunas.Add(new Vacuna
                    {
                        MascotaId = mascota.Id,
                        Nombre = v.Nombre.Trim(),
                        FechaAplicacion = ahora,
                        ProximaDosis = v.ProximaDosis
                    });
                }

                foreach (var c in dto.Cirugias.Where(c => !string.IsNullOrWhiteSpace(c.Tipo)))
                {
                    atencion.Cirugias.Add(new Cirugia
                    {
                        MascotaId = mascota.Id,
                        Tipo = c.Tipo.Trim(),
                        Fecha = ahora,
                        Observaciones = c.Observaciones
                    });
                }

                foreach (var r in dto.Recetas)
                {
                    atencion.Recetas.Add(new Receta
                    {
                        MedicamentoId = r.MedicamentoId,
                        Cantidad = r.Cantidad,
                        Dosis = r.Dosis,
                        Indicaciones = r.Indicaciones
                    });

                    // Descontar stock
                    medicamentos[r.MedicamentoId].StockActual -= r.Cantidad;
                }

                // Las alergias son de la mascota (no de la atención); no se repiten
                var alergiasActuales = await _context.Alergias
                    .Where(a => a.MascotaId == mascota.Id)
                    .Select(a => a.Descripcion.ToLower())
                    .ToListAsync();

                foreach (var a in dto.Alergias.Where(a => !string.IsNullOrWhiteSpace(a.Descripcion)))
                {
                    if (alergiasActuales.Contains(a.Descripcion.Trim().ToLower())) continue;

                    _context.Alergias.Add(new Alergia
                    {
                        MascotaId = mascota.Id,
                        Descripcion = a.Descripcion.Trim(),
                        FechaRegistro = ahora
                    });
                }

                if (dto.Peso.HasValue) mascota.Peso = dto.Peso;
                if (turno != null) turno.Estado = Turno.Atendido;

                _context.Atenciones.Add(atencion);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditoria.RegistrarAsync("ATENCION_REGISTRADA", "Atencion", atencion.Id,
                    $"Mascota: {mascota.Nombre}; Vacunas: {atencion.Vacunas.Count}; Recetas: {atencion.Recetas.Count}; Cirugías: {atencion.Cirugias.Count}");

                return StatusCode(201, new { atencion.Id, mensaje = "Atención registrada." });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
