using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using System.Security.Claims;

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

        private int ObterUsuarioId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (!int.TryParse(claim, out var id))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido");
            }
            return id;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarAvaliacao([FromBody] CriarAvaliacaoDTO avaliacaoDTO)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var avaliacaoCriada = await _avaliacaoServices.CriarAvaliacao(avaliacaoDTO, usuarioId);
                return CreatedAtAction(nameof(ObterAvaliacaoPorId), new { id = avaliacaoCriada.AvaliacaoId }, avaliacaoCriada);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
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
                int? usuarioId = User.Identity?.IsAuthenticated == true ? ObterUsuarioId() : null;
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoes(usuarioId);
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
                int? usuarioId = User.Identity?.IsAuthenticated == true ? ObterUsuarioId() : null;
                var avaliacao = await _avaliacaoServices.ObterAvaliacaoPorId(id, usuarioId);
                return avaliacao != null ? Ok(avaliacao) : NotFound(new { message = "Avaliação não encontrada" });
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
                int? usuarioSolicitanteId = User.Identity?.IsAuthenticated == true ? ObterUsuarioId() : null;
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorUsuario(usuarioId, usuarioSolicitanteId);
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao listar avaliações do usuário: {ex.Message}" });
            }
        }

        [HttpGet("jogo/{jogoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarAvaliacoesPorJogo(int jogoId)
        {
            try
            {
                int? usuarioId = User.Identity?.IsAuthenticated == true ? ObterUsuarioId() : null;
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorJogo(jogoId, usuarioId);
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao listar avaliações de jogo: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
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
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
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
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao excluir avaliação: " + ex.Message });
            }
        }

        [HttpPost("{avaliacaoId}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirAvaliacao(int avaliacaoId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var (curtido, totalCurtidas) = await _avaliacaoServices.AlternarCurtida(avaliacaoId, usuarioId);

                return Ok(new
                {
                    message = curtido ? "Avaliação curtida com sucesso" : "Curtida removida com sucesso",
                    curtido,
                    totalCurtidas
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{avaliacaoId}/curtir")]
        [Authorize]
        public async Task<IActionResult> RemoverCurtida(int avaliacaoId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var sucesso = await _avaliacaoServices.RemoverCurtida(avaliacaoId, usuarioId);

                var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
                return Ok(new
                {
                    message = "Curtida removida com sucesso",
                    curtido = false,
                    totalCurtidas
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{avaliacaoId}/curtidas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterCurtidas(int avaliacaoId)
        {
            try
            {
                var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
                return Ok(new { totalCurtidas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{avaliacaoId}/curtida-status")]
        [Authorize]
        public async Task<IActionResult> VerificarCurtidaUsuario(int avaliacaoId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var curtida = await _avaliacaoServices.UsuarioCurtiu(avaliacaoId, usuarioId);
                return Ok(new { curtida });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ======================= RESPOSTAS DE AVALIAÇÃO ======================= //

        [HttpGet("{avaliacaoId}/respostas")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarRespostas(int avaliacaoId)
        {
            try
            {
                int? usuarioId = User.Identity?.IsAuthenticated == true ? ObterUsuarioId() : null;
                var respostas = await _avaliacaoServices.ListarRespostasPorAvaliacao(avaliacaoId, usuarioId);
                return Ok(respostas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao listar respostas da avaliação: {ex.Message}" });
            }
        }

        [HttpPost("{avaliacaoId}/respostas")]
        [Authorize]
        public async Task<IActionResult> AdicionarResposta(int avaliacaoId, [FromBody] CriarRespostaDTO dto)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var resposta = await _avaliacaoServices.AdicionarResposta(avaliacaoId, usuarioId, dto);
                return CreatedAtAction(nameof(ListarRespostas), new { avaliacaoId }, resposta);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao adicionar resposta: {ex.Message}" });
            }
        }

        [HttpDelete("respostas/{respostaId}")]
        [Authorize]
        public async Task<IActionResult> DeletarResposta(int respostaId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var sucesso = await _avaliacaoServices.DeletarResposta(respostaId, usuarioId);

                return sucesso
                    ? NoContent()
                    : NotFound(new { message = "Resposta não encontrada ou você não tem permissão para excluí-la" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao excluir resposta: {ex.Message}" });
            }
        }

        [HttpPost("respostas/{respostaId}/curtir")]
        [Authorize]
        public async Task<IActionResult> CurtirResposta(int respostaId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var (curtido, totalCurtidas) = await _avaliacaoServices.AlternarCurtidaResposta(respostaId, usuarioId);

                return Ok(new
                {
                    message = curtido ? "Resposta curtida com sucesso" : "Curtida removida com sucesso",
                    curtido,
                    totalCurtidas
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Não autorizado" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
