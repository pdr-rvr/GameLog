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
        private readonly IUsuarioService _usuarioServices;
        private readonly IAuthService _authService;

        public UsuariosController(IUsuarioService usuarioServices, IAuthService authService)
        {
            _usuarioServices = usuarioServices;
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTodosUsuarios()
        {
            var usuarios = await _usuarioServices.ListarUsuarios();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterUsuarioPorId(Guid id)
        {
            var usuario = await _usuarioServices.ObterUsuarioPorId(id);
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
            var generos = await _usuarioServices.IdentificaTopNGenerosFavoritos(id, topN);
            return Ok(generos);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost]
        [HttpPost("registrar")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDTO usuarioDTO)
        {
            var usuario = await _usuarioServices.CriarUsuario(usuarioDTO);
            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.UsuarioId }, usuario);
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO loginDTO)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (usuario, token, refreshToken, expiraEm) = await _authService.AutenticarUsuario(loginDTO, ipAddress);

            if (usuario == null || token == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas ou usuário desativado" });
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                DefinirCookieRefreshToken(refreshToken);
            }

            return Ok(new
            {
                Usuario = usuario,
                Token = token,
                ExpiraEm = expiraEm
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
                var (usuario, novoToken, novoRefreshToken, expiraEm) = await _authService.RenovarTokenAsync(refreshToken, ipAddress);
                DefinirCookieRefreshToken(novoRefreshToken);

                return Ok(new
                {
                    Usuario = usuario,
                    Token = novoToken,
                    ExpiraEm = expiraEm
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
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/usuarios"
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void RemoverCookieRefreshToken()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
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

            var usuarioAtualizado = await _usuarioServices.EditarUsuario(
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

            var sucesso = await _usuarioServices.DeletarUsuario(id, deletarUsuarioDTO.Senha);

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
            var recomendacoes = await _usuarioServices.RecomendarJogos(id);
            return Ok(recomendacoes);
        }

        // ======================= SISTEMA SOCIAL (SEGUIR & FEED) ======================= //

        [HttpPost("{id}/seguir")]
        [Authorize]
        public async Task<IActionResult> AlternarSeguir(Guid id)
        {
            var seguidorId = User.GetUserId();
            var (seguido, totalSeguidores) = await _usuarioServices.AlternarSeguirUsuario(seguidorId, id);

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
            var seguido = await _usuarioServices.VerificarSeSegue(seguidorId, id);
            return Ok(new { seguido });
        }

        [HttpGet("{id}/estatisticas-sociais")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEstatisticasSociais(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var stats = await _usuarioServices.ObterEstatisticasSociais(id, solicitanteId);
            return Ok(stats);
        }

        [HttpGet("{id}/seguidores")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterSeguidores(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var seguidores = await _usuarioServices.ObterSeguidores(id, solicitanteId);
            return Ok(seguidores);
        }

        [HttpGet("{id}/seguindo")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterSeguindo(Guid id)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var seguindo = await _usuarioServices.ObterSeguindo(id, solicitanteId);
            return Ok(seguindo);
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> ObterFeedSocial([FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 20)
        {
            var usuarioId = User.GetUserId();
            var feed = await _usuarioServices.ObterFeedSocial(usuarioId, pagina, itensPorPagina);
            return Ok(feed);
        }

        [HttpGet("atividades")]
        [Authorize]
        public async Task<IActionResult> ObterAtividadesTimeline([FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 30)
        {
            var usuarioId = User.GetUserId();
            var timeline = await _usuarioServices.ObterTimelineAtividades(usuarioId, pagina, itensPorPagina);
            return Ok(timeline);
        }
    }
}
