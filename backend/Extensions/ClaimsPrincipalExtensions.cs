using System;
using System.Security.Claims;

namespace GameLog_Backend.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtém o ID do usuário autenticado a partir dos claims JWT.
        /// Lança UnauthorizedAccessException se o usuário não estiver autenticado ou a claim for inválida.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.GetUserIdOrNull();
            if (!id.HasValue)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado ou identificação inválida no token.");
            }
            return id.Value;
        }

        /// <summary>
        /// Obtém o ID do usuário autenticado a partir dos claims JWT, ou null se não autenticado.
        /// </summary>
        public static Guid? GetUserIdOrNull(this ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var claim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                     ?? user.FindFirst("id")?.Value
                     ?? user.FindFirst("sub")?.Value;

            if (Guid.TryParse(claim, out var id))
            {
                return id;
            }

            return null;
        }

        /// <summary>
        /// Obtém o nome de usuário (username) a partir dos claims JWT.
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("nomeUsuario")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue(ClaimTypes.GivenName);
        }
    }
}
