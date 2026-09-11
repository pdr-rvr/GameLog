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
    public class AvaliacoesController : ControllerBase
    {
        private readonly AvaliacaoServices _avaliacaoServices;

        public AvaliacoesController(AvaliacaoServices avaliacaoServices)
        {
            _avaliacaoServices = avaliacaoServices;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CriarAvaliacaoDTO avaliacaoDTO)
        {
            var usuarioId = User.GetUserId();
            var avaliacaoCriada = await _avaliacaoServices.CriarAvaliacao(avaliacaoDTO, usuarioId);
            return CreatedAtAction(nameof(ObterAvaliacaoPorId), new { id = avaliacaoCriada.AvaliacaoId }, avaliacaoCriada);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoes(
            [FromQuery] int? nota,
            [FromQuery] string? ordenacao,
            [FromQuery] int? pagina,
            [FromQuery] int? itensPorPagina)
        {
            var usuarioId = User.GetUserIdOrNull();
            var avaliacoes = await _avaliacaoServices.ListarAvaliacoes(usuarioId, nota, ordenacao, pagina, itensPorPagina);
            return Ok(avaliacoes);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterAvaliacaoPorId(Guid id)
        {
            var usuarioId = User.GetUserIdOrNull();
            var avaliacao = await _avaliacaoServices.ObterAvaliacaoPorId(id, usuarioId);
            if (avaliacao == null)
            {
                return NotFound(new { message = "Avaliação não encontrada" });
            }
            return Ok(avaliacao);
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoesPorUsuario(Guid usuarioId)
        {
            var usuarioSolicitanteId = User.GetUserIdOrNull();
            var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorUsuario(usuarioId, usuarioSolicitanteId);
            return Ok(avaliacoes);
        }

        [HttpGet("jogo/{jogoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoesPorJogo(Guid jogoId)
        {
            var usuarioId = User.GetUserIdOrNull();
            var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorJogo(jogoId, usuarioId);
            return Ok(avaliacoes);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditarAvaliacao(Guid id, [FromBody] EditarAvaliacaoDTO avaliacaoDTO)
        {
            var usuarioId = User.GetUserId();
            var avaliacaoAtualizada = await _avaliacaoServices.EditarAvaliacao(id, avaliacaoDTO, usuarioId);

            if (avaliacaoAtualizada == null)
            {
                return NotFound(new { message = "Avaliação não encontrada ou você não tem permissão para editá-la" });
            }

            return Ok(avaliacaoAtualizada);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletarAvaliacao(Guid id)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _avaliacaoServices.DeletarAvaliacao(id, usuarioId);

            if (!sucesso)
            {
                return NotFound(new { message = "Avaliação não encontrada ou você não tem permissão para excluí-la" });
            }

            return NoContent();
        }

        [HttpPost("{avaliacaoId}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirAvaliacao(Guid avaliacaoId)
        {
            var usuarioId = User.GetUserId();
            var (curtido, totalCurtidas) = await _avaliacaoServices.AlternarCurtida(avaliacaoId, usuarioId);

            return Ok(new
            {
                message = curtido ? "Avaliação curtida com sucesso" : "Curtida removida com sucesso",
                curtido,
                totalCurtidas
            });
        }

        [HttpDelete("{avaliacaoId}/curtir")]
        [Authorize]
        public async Task<IActionResult> RemoverCurtida(Guid avaliacaoId)
        {
            var usuarioId = User.GetUserId();
            await _avaliacaoServices.RemoverCurtida(avaliacaoId, usuarioId);
            var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);

            return Ok(new
            {
                message = "Curtida removida com sucesso",
                curtido = false,
                totalCurtidas
            });
        }

        [HttpGet("{avaliacaoId}/curtidas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterCurtidas(Guid avaliacaoId)
        {
            var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
            return Ok(new { totalCurtidas });
        }

        [HttpGet("{avaliacaoId}/curtida-status")]
        [Authorize]
        public async Task<IActionResult> VerificarCurtidaUsuario(Guid avaliacaoId)
        {
            var usuarioId = User.GetUserId();
            var curtida = await _avaliacaoServices.UsuarioCurtiu(avaliacaoId, usuarioId);
            return Ok(new { curtida });
        }

        // ======================= RESPOSTAS DE AVALIAÇÃO ======================= //

        [HttpGet("{avaliacaoId}/respostas")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarRespostas(Guid avaliacaoId)
        {
            var usuarioId = User.GetUserIdOrNull();
            var respostas = await _avaliacaoServices.ListarRespostasPorAvaliacao(avaliacaoId, usuarioId);
            return Ok(respostas);
        }

        [HttpPost("{avaliacaoId}/respostas")]
        [Authorize]
        public async Task<IActionResult> AdicionarResposta(Guid avaliacaoId, [FromBody] CriarRespostaDTO dto)
        {
            var usuarioId = User.GetUserId();
            var resposta = await _avaliacaoServices.AdicionarResposta(avaliacaoId, usuarioId, dto);
            return CreatedAtAction(nameof(ListarRespostas), new { avaliacaoId }, resposta);
        }

        [HttpDelete("respostas/{respostaId}")]
        [Authorize]
        public async Task<IActionResult> DeletarResposta(Guid respostaId)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _avaliacaoServices.DeletarResposta(respostaId, usuarioId);

            if (!sucesso)
            {
                return NotFound(new { message = "Resposta não encontrada ou você não tem permissão para excluí-la" });
            }

            return NoContent();
        }

        [HttpPost("respostas/{respostaId}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirResposta(Guid respostaId)
        {
            var usuarioId = User.GetUserId();
            var (curtido, totalCurtidas) = await _avaliacaoServices.AlternarCurtidaResposta(respostaId, usuarioId);

            return Ok(new
            {
                message = curtido ? "Resposta curtida com sucesso" : "Curtida removida com sucesso",
                curtido,
                totalCurtidas
            });
        }
    }
}
