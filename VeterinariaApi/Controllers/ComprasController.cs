
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
    [Authorize(Roles = Roles.Admin)] // Compras de medicamentos a distribuidores: solo administradores
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuditoriaService _auditoria;

        public ComprasController(DataContext context, IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        // GET: api/Compras?pagina=1&tamanoPagina=10&buscar=distribuidor&ordenarPor=fecha_desc
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<CompraResponseDto>>> GetCompras(
            [FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Compras
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda opcional por distribuidor o usuario
            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();

                query = query.Where(c =>
                    c.Distribuidor.RazonSocial.ToLower().Contains(termino) ||
                    c.Usuario.Nombre.ToLower().Contains(termino));
            }

            // Total antes de paginar
            var totalRegistros = await query.CountAsync();

            // Ordenamiento
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "fecha" => query.OrderBy(c => c.Fecha),
                "fecha_desc" => query.OrderByDescending(c => c.Fecha),
                "total_asc" => query.OrderBy(c => c.Total),
                "total_desc" => query.OrderByDescending(c => c.Total),
                _ => query.OrderBy(c => c.Id)
            };

            // Paginación
            var compras = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    DistribuidorId = c.DistribuidorId,
                    DistribuidorNombre = c.Distribuidor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        MedicamentoId = d.MedicamentoId,
                        MedicamentoNombre = d.Medicamento.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            var respuesta = new RespuestaPaginadaDto<CompraResponseDto>(
                compras,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina
            );

            return Ok(respuesta);
        }

        // GET: api/Compras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraResponseDto>> GetCompra(int id)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Distribuidor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Medicamento)
                .Where(c => c.Id == id)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    DistribuidorId = c.DistribuidorId,
                    DistribuidorNombre = c.Distribuidor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        MedicamentoId = d.MedicamentoId,
                        MedicamentoNombre = d.Medicamento.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                return NotFound(new
                {
                    mensaje = "Compra no encontrada."
                });
            }

            return Ok(compra);
        }

        // POST: api/Compras
        [HttpPost]
        public async Task<ActionResult<CompraResponseDto>> CrearCompra(
            CompraRequestDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje = "La compra debe contener al menos un medicamento."
                });
            }

            var distribuidor = await _context.Distribuidores
                .FirstOrDefaultAsync(p => p.Id == dto.DistribuidorId && p.Activo);

            if (distribuidor == null)
            {
                return BadRequest(new
                {
                    mensaje = "El distribuidor indicado no existe."
                });
            }

            // El usuario que registra el ingreso sale del token JWT
            var usuarioId = User.ObtenerUsuarioId();

            if (usuarioId == null)
            {
                return Unauthorized(new
                {
                    mensaje = "Token inválido o usuario no identificado."
                });
            }

            // Transacción: compra + detalles + stock deben guardarse juntos
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = new Compra
                {
                    DistribuidorId = dto.DistribuidorId,
                    UsuarioId = usuarioId.Value,
                    Fecha = Hora.Ahora,
                    Total = 0
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var detalleDto in dto.Detalles)
                {
                    if (detalleDto.Cantidad <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "La cantidad debe ser mayor a cero."
                        });
                    }

                    if (detalleDto.PrecioUnitario <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "El precio unitario debe ser mayor a cero."
                        });
                    }

                    var medicamento = await _context.Medicamentos
                        .FirstOrDefaultAsync(m => m.Id == detalleDto.MedicamentoId);

                    if (medicamento == null)
                    {
                        return BadRequest(new
                        {
                            mensaje = $"El medicamento con ID {detalleDto.MedicamentoId} no existe."
                        });
                    }

                    var detalle = new DetalleCompra
                    {
                        CompraId = compra.Id,
                        MedicamentoId = medicamento.Id,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = detalleDto.PrecioUnitario
                    };

                    _context.DetallesCompras.Add(detalle);

                    // Actualización automática del inventario
                    medicamento.StockActual += detalleDto.Cantidad;

                    total += detalleDto.Cantidad * detalleDto.PrecioUnitario;
                }

                compra.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditoria.RegistrarAsync(
                    "COMPRA_CREADA",
                    "Compra",
                    compra.Id,
                    $"Distribuidor: {distribuidor.RazonSocial}; Ítems: {dto.Detalles.Count}; Total: {total:N2}");

                return CreatedAtAction(
                    nameof(GetCompra),
                    new { id = compra.Id },
                    new
                    {
                        mensaje = "Compra registrada correctamente.",
                        compraId = compra.Id,
                        total = compra.Total
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
