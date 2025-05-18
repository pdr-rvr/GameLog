using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using System.Security.Claims;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvaliacoesController : ControllerBase
    {
        private readonly AvaliacaoServices _avaliacaoServices;

        public AvaliacoesController(AvaliacaoServices avaliacaoServices)
        {
            _avaliacaoServices = avaliacaoServices;
        }

        private int ObterUsuarioId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpPost]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CriarAvaliacaoDTO avaliacaoDTO)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var avaliacaoCriada = await _avaliacaoServices.CriarAvaliacao(avaliacaoDTO, usuarioId);
                return CreatedAtAction(nameof(ObterAvaliacaoPorId), new { id = avaliacaoCriada.AvaliacaoId }, avaliacaoCriada);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoes()
        {
            try
            {
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoes();
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao listar avaliações: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterAvaliacaoPorId(int id)
        {
            try
            {
                var avaliacao = await _avaliacaoServices.ObterAvaliacaoPorId(id);
                return avaliacao != null ? Ok(avaliacao) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao obter avaliação: " + ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoesPorUsuario(int usuarioId)
        {
            try
            {
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorUsuario(usuarioId);
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao listar avaliações do usuário: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarAvaliacao(int id, [FromBody] EditarAvaliacaoDTO avaliacaoDTO)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var avaliacaoAtualizada = await _avaliacaoServices.EditarAvaliacao(id, avaliacaoDTO, usuarioId);

                return avaliacaoAtualizada != null
                    ? Ok(avaliacaoAtualizada)
                    : NotFound(new { message = "Avaliação não encontrada ou você não tem permissão para editá-la" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarAvaliacao(int id)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var sucesso = await _avaliacaoServices.DeletarAvaliacao(id, usuarioId);

                return sucesso
                    ? NoContent()
                    : NotFound(new { message = "Avaliação não encontrada ou você não tem permissão para excluí-la" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao excluir avaliação: " + ex.Message });
            }
        }
    }
}