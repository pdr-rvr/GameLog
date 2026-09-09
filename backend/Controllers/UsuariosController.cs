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
        public async Task<IActionResult> ObterUsuarioPorId(int id)
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
        public async Task<IActionResult> ObterGenerosFavoritos(int id, [FromQuery] int topN = 4)
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
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] EditarUsuarioDTO editarUsuarioDTO)
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
        public async Task<IActionResult> DeletarUsuario(int id, [FromBody] DeletarUsuarioDTO deletarUsuarioDTO)
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
        public async Task<IActionResult> ObterRecomendacoes(int id)
        {
            var recomendacoes = await _usuarioServices.RecomendarJogos(id);
            return Ok(recomendacoes);
        }
    }
}
