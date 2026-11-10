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
    // Gestión de usuarios del personal: solo administradores.
    // El admin inicial (sembrado en la base) crea a los veterinarios y recepcionistas.
    [Authorize(Roles = Roles.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAuditoriaService _auditoria;

        public UsuariosController(
            DataContext context,
            IPasswordHasher<Usuario> hasher,
            IAuditoriaService auditoria)
        {
            _context = context;
            _hasher = hasher;
            _auditoria = auditoria;
        }

        // GET: api/Usuarios?rol=Veterinario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetUsuarios([FromQuery] string? rol)
        {
            var query = _context.Usuarios.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(rol))
            {
                query = query.Where(u => u.Rol == rol);
            }

            var usuarios = await query
                .OrderBy(u => u.Rol)
                .ThenBy(u => u.Nombre)
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Username = u.Username,
                    Rol = u.Rol,
                    Email = u.Email,
                    Activo = u.Activo,
                    VeterinarioId = u.Veterinario != null ? (int?)u.Veterinario.Id : null,
                    Matricula = u.Veterinario != null ? u.Veterinario.Matricula : null,
                    EspecialidadNombre = u.Veterinario != null ? u.Veterinario.Especialidad.Nombre : null
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // POST: api/Usuarios  (crea Admin, Veterinario o Recepcionista)
        [HttpPost]
        public async Task<ActionResult<UsuarioResponseDto>> CrearUsuario(UsuarioRequestDto dto)
        {
            if (!Roles.RolesDePersonal.Contains(dto.Rol))
            {
                return BadRequest(new
                {
                    mensaje = "Rol inválido. Permitidos: Admin, Veterinario, Recepcionista. Las cuentas de dueños las crea recepción."
                });
            }

            var username = dto.Username.Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Username == username))
                return BadRequest(new { mensaje = "El nombre de usuario ya está en uso." });

            Especialidad? especialidad = null;

            if (dto.Rol == Roles.Veterinario)
            {
                if (string.IsNullOrWhiteSpace(dto.Matricula) || dto.EspecialidadId is null or <= 0)
                    return BadRequest(new { mensaje = "Para un veterinario la matrícula y la especialidad son obligatorias." });

                var matricula = dto.Matricula.Trim();

                if (await _context.Veterinarios.AnyAsync(v => v.Matricula == matricula))
                    return BadRequest(new { mensaje = "Ya existe un veterinario con esa matrícula." });

                especialidad = await _context.Especialidades
                    .FirstOrDefaultAsync(e => e.Id == dto.EspecialidadId && e.Activo);

                if (especialidad == null)
                    return BadRequest(new { mensaje = "La especialidad indicada no existe." });
            }

            // Usuario y ficha de veterinario se guardan juntos
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Username = username,
                Rol = dto.Rol,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
                Activo = true
            };
            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            Veterinario? veterinario = null;

            if (especialidad != null)
            {
                veterinario = new Veterinario
                {
                    UsuarioId = usuario.Id,
                    Matricula = dto.Matricula!.Trim(),
                    Telefono = dto.Telefono,
                    EspecialidadId = especialidad.Id
                };

                _context.Veterinarios.Add(veterinario);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            await _auditoria.RegistrarAsync(
                "USUARIO_CREADO", "Usuario", usuario.Id, $"Username: {username}; Rol: {usuario.Rol}");

            return StatusCode(201, new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                Email = usuario.Email,
                Activo = usuario.Activo,
                VeterinarioId = veterinario?.Id,
                Matricula = veterinario?.Matricula,
                EspecialidadNombre = especialidad?.Nombre
            });
        }

        // PUT: api/Usuarios/5/activo?activo=false
        [HttpPut("{id}/activo")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
        {
            if (User.ObtenerUsuarioId() == id)
                return BadRequest(new { mensaje = "No podés cambiar el estado de tu propio usuario." });

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                activo ? "USUARIO_ACTIVADO" : "USUARIO_DESACTIVADO", "Usuario", usuario.Id, $"Username: {usuario.Username}");

            return NoContent();
        }

        // PUT: api/Usuarios/5/password   (el admin blanquea la contraseña de alguien)
        [HttpPut("{id}/password")]
        public async Task<IActionResult> RestablecerPassword(int id, PasswordRequestDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("PASSWORD_RESTABLECIDA", "Usuario", usuario.Id, $"Username: {usuario.Username}");

            return NoContent();
        }
    }
}
