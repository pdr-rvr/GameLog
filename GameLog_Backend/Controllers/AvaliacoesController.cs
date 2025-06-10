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
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
                int? usuarioId = User.Identity.IsAuthenticated ? ObterUsuarioId() : null;
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
                int? usuarioId = User.Identity.IsAuthenticated ? ObterUsuarioId() : null;
                var avaliacao = await _avaliacaoServices.ObterAvaliacaoPorId(id, usuarioId);
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
                int? usuarioSolicitanteId = User.Identity.IsAuthenticated ? ObterUsuarioId() : null;
                var avaliacoes = await _avaliacaoServices.ListarAvaliacoesPorUsuario(usuarioId, usuarioSolicitanteId);
                return Ok(avaliacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro ao listar avaliações do usuário: {ex.Message}" });
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao excluir avaliação: " + ex.Message });
            }
        }

        //[HttpPost("{avaliacaoId}/curtir")]
        //[Authorize]
        //public async Task<IActionResult> CurtirAvaliacao(int avaliacaoId)
        //{
        //    try
        //    {
        //        var usuarioId = ObterUsuarioId();
        //        var sucesso = await _avaliacaoServices.AdicionarCurtida(avaliacaoId, usuarioId);

        //        if (!sucesso)
        //            return BadRequest(new { message = "Não foi possível curtir a avaliação" });

        //        var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
        //        return Ok(new
        //        {
        //            message = "Avaliação curtida com sucesso",
        //            totalCurtidas
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        //[HttpDelete("{avaliacaoId}/curtir")]
        //[Authorize]
        //public async Task<IActionResult> RemoverCurtida(int avaliacaoId)
        //{
        //    try
        //    {
        //        var usuarioId = ObterUsuarioId();
        //        var sucesso = await _avaliacaoServices.RemoverCurtida(avaliacaoId, usuarioId);

        //        if (!sucesso)
        //            return BadRequest(new { message = "Não foi possível remover a curtida" });

        //        var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
        //        return Ok(new
        //        {
        //            message = "Curtida removida com sucesso",
        //            totalCurtidas
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        //[HttpGet("{avaliacaoId}/curtidas")]
        //[AllowAnonymous]
        //public async Task<IActionResult> ObterCurtidas(int avaliacaoId)
        //{
        //    try
        //    {
        //        var totalCurtidas = await _avaliacaoServices.ContarCurtidas(avaliacaoId);
        //        return Ok(new { totalCurtidas });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        //[HttpGet("{avaliacaoId}/curtida-status")]
        //[Authorize]
        //public async Task<IActionResult> VerificarCurtidaUsuario(int avaliacaoId)
        //{
        //    try
        //    {
        //        var usuarioId = ObterUsuarioId();
        //        var curtida = await _avaliacaoServices.UsuarioCurtiu(avaliacaoId, usuarioId);
        //        return Ok(new { curtida });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}
    }
}