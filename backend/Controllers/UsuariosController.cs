using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        private int? ObterUsuarioIdAutenticado()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTodosUsuarios()
        {
            try
            {
                var usuarios = await _usuarioServices.ListarUsuarios();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterUsuarioPorId(int id)
        {
            try
            {
                var usuario = await _usuarioServices.ObterUsuarioPorId(id);
                return usuario != null ? Ok(usuario) : NotFound(new { message = "Usuário não encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("{id}/generos-favoritos")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterGenerosFavoritos(int id, [FromQuery] int topN = 4)
        {
            try
            {
                var generos = await _usuarioServices.IdentificaTopNGenerosFavoritos(id, topN);
                return Ok(generos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar gêneros favoritos: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDTO usuarioDTO)
        {
            try
            {
                var usuario = await _usuarioServices.CriarUsuario(usuarioDTO);
                return CreatedAtAction(nameof(ObterUsuarioPorId), new { id = usuario.UsuarioId }, usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO loginDTO)
        {
            try
            {
                var result = await _usuarioServices.AutenticarUsuario(loginDTO);

                if (result.usuario == null || result.token == null)
                    return Unauthorized(new { message = "Credenciais inválidas ou usuário desativado" });

                return Ok(new
                {
                    Usuario = result.usuario,
                    Token = result.token,
                    ExpiraEm = result.expiraEm
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] EditarUsuarioDTO editarUsuarioDTO)
        {
            try
            {
                var usuarioIdAutenticado = ObterUsuarioIdAutenticado();
                if (usuarioIdAutenticado == null || usuarioIdAutenticado != id)
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarUsuario(int id, [FromBody] DeletarUsuarioDTO deletarUsuarioDTO)
        {
            try
            {
                var usuarioIdAutenticado = ObterUsuarioIdAutenticado();
                if (usuarioIdAutenticado == null || usuarioIdAutenticado != id)
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("{id}/recomendacoes")]
        [Authorize]
        public async Task<IActionResult> ObterRecomendacoes(int id)
        {
            try
            {
                var recomendacoes = await _usuarioServices.RecomendarJogos(id);
                return Ok(recomendacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar recomendações: " + ex.Message });
            }
        }
    }
}
