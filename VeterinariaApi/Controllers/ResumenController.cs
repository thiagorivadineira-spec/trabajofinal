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
    // Indicadores para la pantalla de inicio de la web
    [Authorize(Roles = Roles.Personal)]
    [Route("api/[controller]")]
    [ApiController]
    public class ResumenController : ControllerBase
    {
        private readonly DataContext _context;

        public ResumenController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Resumen
        [HttpGet]
        public async Task<ActionResult<ResumenDto>> Get()
        {
            var hoy = Hora.Hoy;
            var manana = hoy.AddDays(1);

            var turnosHoy = _context.Turnos.Where(t => t.FechaHora >= hoy && t.FechaHora < manana && t.Estado != Turno.Cancelado);

            return Ok(new ResumenDto
            {
                TurnosHoy = await turnosHoy.CountAsync(),
                TurnosPendientesHoy = await turnosHoy.CountAsync(t => t.Estado == Turno.Pendiente),
                Mascotas = await _context.Mascotas.CountAsync(m => m.Activo),
                Duenos = await _context.Duenos.CountAsync(d => d.Activo),
                Veterinarios = await _context.Veterinarios.CountAsync(v => v.Usuario.Activo),
                MedicamentosAReponer = await _context.Medicamentos.CountAsync(m => m.Activo && m.StockActual <= m.StockMinimo),
                AtencionesHoy = await _context.Atenciones.CountAsync(a => a.Fecha >= hoy && a.Fecha < manana)
            });
        }
    }
}
