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
        public async Task<IActionResult> ListarTodosJogos()
        {
            try
            {
                var jogos = await _jogoServices.ListarJogos();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
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
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
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
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
