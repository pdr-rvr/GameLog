using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Extensions;
using GameLog_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAuthService _authService;
        private readonly ISocialService _socialService;
        private readonly IFeedService _feedService;

        public UsuariosController(
            IUserProfileService userProfileService,
            IAuthService authService,
            ISocialService socialService,
            IFeedService feedService)
        {
            _userProfileService = userProfileService;
            _authService = authService;
            _socialService = socialService;
            _feedService = feedService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTodosUsuarios()
        {
            var usuarios = await _userProfileService.ListarUsuarios();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterUsuarioPorId(Guid id)
        {
            var usuario = await _userProfileService.ObterUsuarioPorId(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            return Ok(usuario);
        }

        [HttpGet("{id}/generos-favoritos")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterGenerosFavoritos(Guid id, [FromQuery] int topN = 4)
        {
            var generos = await _userProfileService.IdentificaTopNGenerosFavoritos(id, topN);
            return Ok(generos);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost]
        [HttpPost("registrar")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDTO usuarioDTO)
        {
            var usuario = await _authService.RegistrarUsuario(usuarioDTO);
            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.UsuarioId }, usuario);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO loginDTO)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var authResult = await _authService.AutenticarUsuario(loginDTO, ipAddress);

            if (!authResult.Sucesso)
            {
                return Unauthorized(new { message = "Credenciais inválidas ou usuário desativado" });
            }

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
            {
                DefinirCookieRefreshToken(authResult.RefreshToken);
            }

            return Ok(new
            {
                Usuario = authResult.Usuario,
                Token = authResult.Token,
                ExpiraEm = authResult.ExpiraEm
            });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO? request)
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { message = "Token de atualização não informado." });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var authResult = await _authService.RenovarTokenAsync(refreshToken, ipAddress);
                if (!string.IsNullOrEmpty(authResult.RefreshToken))
                {
                    DefinirCookieRefreshToken(authResult.RefreshToken);
                }

                return Ok(new
                {
                    Usuario = authResult.Usuario,
                    Token = authResult.Token,
                    ExpiraEm = authResult.ExpiraEm
                });
            }
            catch (SecurityTokenException ex)
            {
                RemoverCookieRefreshToken();
                return Unauthorized(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("revogar")]
        public async Task<IActionResult> RevogarToken([FromBody] RefreshTokenRequestDTO? request)
        {
            var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { message = "Token de atualização não informado." });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var sucesso = await _authService.RevogarTokenAsync(refreshToken, ipAddress);
            RemoverCookieRefreshToken();

            if (!sucesso)
            {
                return NotFound(new { message = "Token não encontrado ou já revogado." });
            }

            return Ok(new { message = "Token de atualização revogado com sucesso." });
        }

        private void DefinirCookieRefreshToken(string refreshToken)
        {
            var isProduction = !string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || isProduction,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/usuarios"
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void RemoverCookieRefreshToken()
        {
            var isProduction = !string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps || isProduction,
                SameSite = SameSiteMode.Lax,
                Path = "/api/usuarios"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(Guid id, [FromBody] EditarUsuarioDTO editarUsuarioDTO)
        {
            var usuarioIdAutenticado = User.GetUserId();
            if (usuarioIdAutenticado != id)
            {
                return Forbid();
            }

            var usuarioAtualizado = await _userProfileService.EditarUsuario(
                id,
                editarUsuarioDTO.SenhaAtual,
                editarUsuarioDTO); 

            if (usuarioAtualizado == null)
            {
                return Unauthorized(new { message = "Senha atual incorreta ou usuário não encontrado" });
            }

            return Ok(usuarioAtualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarUsuario(Guid id, [FromBody] DeletarUsuarioDTO deletarUsuarioDTO)
        {
            var usuarioIdAutenticado = User.GetUserId();
            if (usuarioIdAutenticado != id)
            {
                return Forbid();
            }

            var sucesso = await _userProfileService.DeletarUsuario(id, deletarUsuarioDTO.Senha);

            if (!sucesso)
            {
                return Unauthorized(new { message = "Senha incorreta ou usuário não encontrado" });
            }

            return NoContent();
        }

        [HttpGet("{id}/recomendacoes")]
        [Authorize]
        public async Task<IActionResult> ObterRecomendacoes(Guid id)
        {
            var recomendacoes = await _userProfileService.RecomendarJogos(id);
            return Ok(recomendacoes);
        }

        // ======================= SISTEMA SOCIAL (SEGUIR & FEED) ======================= //

        [HttpPost("{id}/seguir")]
        [Authorize]
        public async Task<IActionResult> AlternarSeguir(Guid id)
        {
            var seguidorId = User.GetUserId();
            var (seguido, totalSeguidores) = await _socialService.AlternarSeguirUsuario(seguidorId, id);

            return Ok(new
            {
                seguido,
                totalSeguidores,
                message = seguido ? "Usuário seguido com sucesso" : "Você deixou de seguir este usuário"
            });
        }

        [HttpGet("{id}/status-seguir")]
        [Authorize]
        public async Task<IActionResult> VerificarStatusSeguir(Guid id)
        {
            var seguidorId = User.GetUserId();
            var seguido = await _socialService.VerificarSeSegue(seguidorId, id);
            return Ok(new { seguido });
        }

        [HttpGet("{id}/estatisticas-sociais")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEstatisticasSociais(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var stats = await _socialService.ObterEstatisticasSociais(id, solicitanteId);
            return Ok(stats);
        }

        [HttpGet("{id}/seguidores")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterSeguidores(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var seguidores = await _socialService.ObterSeguidores(id, solicitanteId);
            return Ok(seguidores);
        }

        [HttpGet("{id}/seguindo")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterSeguindo(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var seguindo = await _socialService.ObterSeguindo(id, solicitanteId);
            return Ok(seguindo);
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> ObterFeedSocial([FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 20)
        {
            var usuarioId = User.GetUserId();
            var feed = await _feedService.ObterFeedSocial(usuarioId, pagina, itensPorPagina);
            return Ok(feed);
        }

        [HttpGet("atividades")]
        [Authorize]
        public async Task<IActionResult> ObterAtividadesTimeline([FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 30)
        {
            var usuarioId = User.GetUserId();
            var timeline = await _feedService.ObterTimelineAtividades(usuarioId, pagina, itensPorPagina);
            return Ok(timeline);
        }
    }
}
