using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JogosController : ControllerBase
    {
        private readonly JogoServices _jogoServices;

        public JogosController(JogoServices jogoServices)
        {
            _jogoServices = jogoServices;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTodosJogos(
            [FromQuery] int? pagina,
            [FromQuery] int? itensPorPagina,
            [FromQuery] string? busca,
            [FromQuery] string? genero,
            [FromQuery] int? ano,
            [FromQuery] string? empresa,
            [FromQuery] string? ordenacao)
        {
            try
            {
                // Se pagina for especificada, retorna PagedResult
                if (pagina.HasValue)
                {
                    var paged = await _jogoServices.ListarJogosPaginados(
                        pagina.Value,
                        itensPorPagina ?? 12,
                        busca,
                        genero,
                        ano,
                        empresa,
                        ordenacao ?? "melhores"
                    );
                    return Ok(paged);
                }

                var jogos = await _jogoServices.ListarJogos();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterJogoPorId(int id)
        {
            try
            {
                var jogo = await _jogoServices.ObterJogoPorId(id);

                if (jogo == null)
                    return NotFound(new { message = "Jogo não encontrado" });

                return Ok(jogo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet("top-avaliados")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTop10MelhorAvaliados()
        {
            try
            {
                var jogos = await _jogoServices.ListarTop10JogosMelhorAvaliados();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }
    }
}
