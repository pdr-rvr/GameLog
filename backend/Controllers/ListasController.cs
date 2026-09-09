using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListasController : ControllerBase
    {
        private readonly ListaServices _listaServices;

        public ListasController(ListaServices listaServices)
        {
            _listaServices = listaServices;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst("id")?.Value
                     ?? User.FindFirst("sub")?.Value;

            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarLista([FromBody] CriarListaDTO dto)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var lista = await _listaServices.CriarLista(usuarioId.Value, dto);
                return CreatedAtAction(nameof(ObterListaPorId), new { id = lista.ListaId }, lista);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar lista: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterListaPorId(int id)
        {
            try
            {
                var usuarioId = GetCurrentUserId();
                var lista = await _listaServices.ObterListaPorId(id, usuarioId);
                if (lista == null)
                {
                    return NotFound(new { message = "Lista não encontrada ou privada." });
                }
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter lista: " + ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarListasDoUsuario(int usuarioId)
        {
            try
            {
                var solicitanteId = GetCurrentUserId();
                var listas = await _listaServices.ListarListasDoUsuario(usuarioId, solicitanteId);
                return Ok(listas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar listas do usuário: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditarLista(int id, [FromBody] EditarListaDTO dto)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var lista = await _listaServices.EditarLista(id, usuarioId.Value, dto);
                if (lista == null)
                {
                    return NotFound(new { message = "Lista não encontrada ou sem permissão de edição." });
                }
                return Ok(lista);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao editar lista: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var sucesso = await _listaServices.DeletarLista(id, usuarioId.Value);
                if (!sucesso)
                {
                    return NotFound(new { message = "Lista não encontrada ou sem permissão de exclusão." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir lista: " + ex.Message });
            }
        }

        [HttpPost("{id}/jogos")]
        [Authorize]
        public async Task<IActionResult> AdicionarJogo(int id, [FromBody] AdicionarJogoListaDTO dto)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var sucesso = await _listaServices.AdicionarJogoNaLista(id, usuarioId.Value, dto.JogoId);
                if (!sucesso)
                {
                    return NotFound(new { message = "Lista não encontrada ou sem permissão." });
                }
                return Ok(new { message = "Jogo adicionado à lista com sucesso." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao adicionar jogo à lista: " + ex.Message });
            }
        }

        [HttpDelete("{id}/jogos/{jogoId}")]
        [Authorize]
        public async Task<IActionResult> RemoverJogo(int id, int jogoId)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var sucesso = await _listaServices.RemoverJogoDaLista(id, usuarioId.Value, jogoId);
                if (!sucesso)
                {
                    return NotFound(new { message = "Jogo ou lista não encontrada." });
                }
                return Ok(new { message = "Jogo removido da lista com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao remover jogo da lista: " + ex.Message });
            }
        }
    }
}
