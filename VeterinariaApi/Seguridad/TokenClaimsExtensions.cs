using System.Security.Claims;

namespace VeterinariaApi.Seguridad
{
    public static class TokenClaimsExtensions
    {
        /// <summary>Id del usuario autenticado (claim del token JWT).</summary>
        public static int? ObtenerUsuarioId(this ClaimsPrincipal user)
        {
            var valor = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("nameid")
                        ?? user.FindFirstValue("sub");

            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
