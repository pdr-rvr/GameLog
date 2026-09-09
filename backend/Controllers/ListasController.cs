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
    public class ListasController : ControllerBase
    {
        private readonly ListaServices _listaServices;

        public ListasController(ListaServices listaServices)
        {
            _listaServices = listaServices;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarLista([FromBody] CriarListaDTO dto)
        {
            var usuarioId = User.GetUserId();
            var lista = await _listaServices.CriarLista(usuarioId, dto);
            return CreatedAtAction(nameof(ObterListaPorId), new { id = lista.ListaId }, lista);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterListaPorId(int id)
        {
            var usuarioId = User.GetUserIdOrNull();
            var lista = await _listaServices.ObterListaPorId(id, usuarioId);
            if (lista == null)
            {
                return NotFound(new { message = "Lista não encontrada ou privada." });
            }
            return Ok(lista);
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarListasDoUsuario(int usuarioId)
        {
            var solicitanteId = User.GetUserIdOrNull();
            var listas = await _listaServices.ListarListasDoUsuario(usuarioId, solicitanteId);
            return Ok(listas);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditarLista(int id, [FromBody] EditarListaDTO dto)
        {
            var usuarioId = User.GetUserId();
            var lista = await _listaServices.EditarLista(id, usuarioId, dto);
            if (lista == null)
            {
                return NotFound(new { message = "Lista não encontrada ou sem permissão de edição." });
            }
            return Ok(lista);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletarLista(int id)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _listaServices.DeletarLista(id, usuarioId);
            if (!sucesso)
            {
                return NotFound(new { message = "Lista não encontrada ou sem permissão de exclusão." });
            }
            return NoContent();
        }

        [HttpPost("{id}/jogos")]
        [Authorize]
        public async Task<IActionResult> AdicionarJogo(int id, [FromBody] AdicionarJogoListaDTO dto)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _listaServices.AdicionarJogoNaLista(id, usuarioId, dto.JogoId);
            if (!sucesso)
            {
                return NotFound(new { message = "Lista não encontrada ou sem permissão." });
            }
            return Ok(new { message = "Jogo adicionado à lista com sucesso." });
        }

        [HttpDelete("{id}/jogos/{jogoId}")]
        [Authorize]
        public async Task<IActionResult> RemoverJogo(int id, int jogoId)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _listaServices.RemoverJogoDaLista(id, usuarioId, jogoId);
            if (!sucesso)
            {
                return NotFound(new { message = "Jogo ou lista não encontrada." });
            }
            return Ok(new { message = "Jogo removido da lista com sucesso." });
        }
    }
}
