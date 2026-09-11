using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Extensions;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioServices _usuarioServices;

        public UsuariosController(UsuarioServices usuarioServices)
        {
            _usuarioServices = usuarioServices;
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
        [HttpPost]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDTO usuarioDTO)
        {
            var usuario = await _usuarioServices.CriarUsuario(usuarioDTO);
            return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.UsuarioId }, usuario);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO loginDTO)
        {
            var result = await _usuarioServices.AutenticarUsuario(loginDTO);

            if (result.usuario == null || result.token == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas ou usuário desativado" });
            }

            return Ok(new
            {
                Usuario = result.usuario,
                Token = result.token,
                ExpiraEm = result.expiraEm
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
    }
}
