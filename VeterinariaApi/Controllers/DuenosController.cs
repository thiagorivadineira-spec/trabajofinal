using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;
using VeterinariaApi.Models;
using VeterinariaApi.Seguridad;
using VeterinariaApi.Services;

namespace VeterinariaApi.Controllers
{
    // Recepción registra a los dueños y, si quieren, les crea la cuenta para la app móvil.
    [Authorize(Roles = Roles.AdminORecepcion)]
    [Route("api/[controller]")]
    [ApiController]
    public class DuenosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAuditoriaService _auditoria;

        public DuenosController(DataContext context, IPasswordHasher<Usuario> hasher, IAuditoriaService auditoria)
        {
            _context = context;
            _hasher = hasher;
            _auditoria = auditoria;
        }

        // GET: api/Duenos?pagina=1&tamanoPagina=10&buscar=perez
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<DuenoResponseDto>>> GetDuenos([FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Duenos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();
                query = query.Where(d =>
                    d.Dni.Contains(termino) ||
                    d.Nombre.ToLower().Contains(termino) ||
                    d.Apellido.ToLower().Contains(termino) ||
                    (d.Telefono != null && d.Telefono.Contains(termino)));
            }

            var total = await query.CountAsync();

            var datos = await query
                .OrderBy(d => d.Apellido).ThenBy(d => d.Nombre)
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(d => new DuenoResponseDto
                {
                    Id = d.Id,
                    Dni = d.Dni,
                    Nombre = d.Nombre,
                    Apellido = d.Apellido,
                    NombreCompleto = d.Apellido + ", " + d.Nombre,
                    Telefono = d.Telefono,
                    Email = d.Email,
                    Direccion = d.Direccion,
                    Activo = d.Activo,
                    TieneCuenta = d.UsuarioId != null,
                    Username = d.Usuario != null ? d.Usuario.Username : null,
                    CantidadMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .ToListAsync();

            return Ok(new RespuestaPaginadaDto<DuenoResponseDto>(datos, total, parametros.Pagina, parametros.TamanoPagina));
        }

        // GET: api/Duenos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DuenoResponseDto>> GetDueno(int id)
        {
            var dueno = await _context.Duenos
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DuenoResponseDto
                {
                    Id = d.Id,
                    Dni = d.Dni,
                    Nombre = d.Nombre,
                    Apellido = d.Apellido,
                    NombreCompleto = d.Apellido + ", " + d.Nombre,
                    Telefono = d.Telefono,
                    Email = d.Email,
                    Direccion = d.Direccion,
                    Activo = d.Activo,
                    TieneCuenta = d.UsuarioId != null,
                    Username = d.Usuario != null ? d.Usuario.Username : null,
                    CantidadMascotas = d.Mascotas.Count(m => m.Activo)
                })
                .FirstOrDefaultAsync();

            return dueno == null ? NotFound(new { mensaje = "Dueño no encontrado." }) : Ok(dueno);
        }

        // POST: api/Duenos   (si vienen Username y Password, también crea la cuenta de la app)
        [HttpPost]
        public async Task<IActionResult> Crear(DuenoRequestDto dto)
        {
            var dni = dto.Dni.Trim();

            if (await _context.Duenos.AnyAsync(d => d.Dni == dni))
                return BadRequest(new { mensaje = "Ya existe un dueño con ese DNI." });

            var quiereCuenta = !string.IsNullOrWhiteSpace(dto.Username);

            if (quiereCuenta)
            {
                var error = await ValidarCuentaAsync(dto.Username!, dto.Password);
                if (error != null) return BadRequest(new { mensaje = error });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var dueno = new Dueno
            {
                Dni = dni,
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                Activo = true
            };

            if (quiereCuenta)
            {
                dueno.Usuario = NuevaCuenta(dueno, dto.Username!.Trim(), dto.Password!);
            }

            _context.Duenos.Add(dueno);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _auditoria.RegistrarAsync("DUENO_CREADO", "Dueno", dueno.Id,
                $"DNI: {dni}; Cuenta app: {(quiereCuenta ? dto.Username : "no")}");

            return StatusCode(201, new { dueno.Id });
        }

        // PUT: api/Duenos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, DuenoRequestDto dto)
        {
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno == null)
                return NotFound(new { mensaje = "Dueño no encontrado." });

            var dni = dto.Dni.Trim();

            if (await _context.Duenos.AnyAsync(d => d.Dni == dni && d.Id != id))
                return BadRequest(new { mensaje = "Ya existe otro dueño con ese DNI." });

            dueno.Dni = dni;
            dueno.Nombre = dto.Nombre.Trim();
            dueno.Apellido = dto.Apellido.Trim();
            dueno.Telefono = dto.Telefono;
            dueno.Email = dto.Email;
            dueno.Direccion = dto.Direccion;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("DUENO_EDITADO", "Dueno", id, $"DNI: {dni}");

            return NoContent();
        }

        // POST: api/Duenos/5/cuenta   (crear la cuenta de la app para un dueño que no tenía)
        [HttpPost("{id}/cuenta")]
        public async Task<IActionResult> CrearCuenta(int id, CrearCuentaDuenoDto dto)
        {
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno == null)
                return NotFound(new { mensaje = "Dueño no encontrado." });

            if (dueno.UsuarioId != null)
                return BadRequest(new { mensaje = "Este dueño ya tiene una cuenta." });

            var error = await ValidarCuentaAsync(dto.Username, dto.Password);
            if (error != null) return BadRequest(new { mensaje = error });

            dueno.Usuario = NuevaCuenta(dueno, dto.Username.Trim(), dto.Password);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("CUENTA_DUENO_CREADA", "Dueno", id, $"Username: {dto.Username}");

            return Ok(new { mensaje = "Cuenta creada. El dueño ya puede ingresar a la app." });
        }

        // PUT: api/Duenos/5/activo?activo=false
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            var dueno = await _context.Duenos.Include(d => d.Usuario).FirstOrDefaultAsync(d => d.Id == id);
            if (dueno == null)
                return NotFound(new { mensaje = "Dueño no encontrado." });

            dueno.Activo = activo;
            if (dueno.Usuario != null) dueno.Usuario.Activo = activo;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string?> ValidarCuentaAsync(string username, string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return "La contraseña de la cuenta debe tener al menos 6 caracteres.";

            var nombre = username.Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Username == nombre))
                return "El nombre de usuario ya está en uso.";

            return null;
        }

        private Usuario NuevaCuenta(Dueno dueno, string username, string password)
        {
            var usuario = new Usuario
            {
                Nombre = $"{dueno.Nombre} {dueno.Apellido}",
                Username = username,
                Email = dueno.Email,
                Rol = Roles.Dueno,
                Activo = true
            };
            usuario.PasswordHash = _hasher.HashPassword(usuario, password);
            return usuario;
        }
    }
}
