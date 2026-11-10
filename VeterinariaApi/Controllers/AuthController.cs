using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VeterinariaApi.Data;
using VeterinariaApi.DTOs;
using VeterinariaApi.Models;
using VeterinariaApi.Seguridad;
using VeterinariaApi.Services;

namespace VeterinariaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IAuditoriaService _auditoria;

        public AuthController(
            DataContext context,
            IConfiguration config,
            IPasswordHasher<Usuario> hasher,
            IAuditoriaService auditoria)
        {
            _context = context;
            _config = config;
            _hasher = hasher;
            _auditoria = auditoria;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginDto)
        {
            var username = loginDto.Username.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.Veterinario)
                .Include(u => u.Dueno)
                .FirstOrDefaultAsync(u => u.Username == username);

            var resultado = (usuario == null || !usuario.Activo)
                ? PasswordVerificationResult.Failed
                : VerificarPassword(usuario, loginDto.Password);

            if (usuario == null || resultado == PasswordVerificationResult.Failed)
            {
                await _auditoria.RegistrarAsync(
                    "LOGIN_FALLIDO", "Usuario", usuario?.Id, $"Username: {username}", usuario?.Id);

                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            // Si el algoritmo de hash fue actualizado, se re-hashea con el nuevo
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = _hasher.HashPassword(usuario, loginDto.Password);
                await _context.SaveChangesAsync();
            }

            // --- FABRICACIÓN DEL TOKEN JWT ---
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Los "Claims" son los datos que viajan dentro del token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var expira = DateTime.UtcNow.AddHours(8); // El token dura una jornada de trabajo

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            // ---------------------------------

            await _auditoria.RegistrarAsync("LOGIN_OK", "Usuario", usuario.Id, null, usuario.Id);

            return Ok(new AuthResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                VeterinarioId = usuario.Veterinario?.Id,
                DuenoId = usuario.Dueno?.Id,
                Token = tokenString,
                Expira = expira
            });
        }

        // POST: api/Auth/CambiarPassword
        [Authorize]
        [HttpPost("CambiarPassword")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordRequestDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Unauthorized(new { mensaje = "Token inválido o usuario no identificado." });

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId.Value);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado." });

            if (VerificarPassword(usuario, dto.PasswordActual) == PasswordVerificationResult.Failed)
                return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });

            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.PasswordNueva);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("PASSWORD_CAMBIADA", "Usuario", usuario.Id);

            return Ok(new { mensaje = "Contraseña actualizada." });
        }

        private PasswordVerificationResult VerificarPassword(Usuario usuario, string password)
        {
            try
            {
                return _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            }
            catch (FormatException)
            {
                // El hash guardado no tiene formato válido
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
