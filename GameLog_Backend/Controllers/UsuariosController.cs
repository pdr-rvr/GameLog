using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult ListarTodosUsuarios()
        {
            try
            {
                var usuarios = _usuarioServices.ListarUsuarios();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObterUsuarioPorId(int id)
        {
            try
            {
                var usuario = _usuarioServices.ObterUsuarioPorId(id);
                return usuario != null ? Ok(usuario) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
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
                    return Unauthorized(new { message = "Credenciais inválidas" });

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
				var usuarioAtualizado = await _usuarioServices.EditarUsuario(
					id,
					editarUsuarioDTO.SenhaAtual,
					editarUsuarioDTO); // ← Passa o DTO diretamente

				if (usuarioAtualizado == null)
				{
					return Unauthorized(new { message = "Senha incorreta ou usuário não encontrado" });
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
	}
}