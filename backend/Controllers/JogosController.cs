using System.Threading.Tasks;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterJogoPorId(int id)
        {
            var jogo = await _jogoServices.ObterJogoPorId(id);
            if (jogo == null)
            {
                return NotFound(new { message = "Jogo não encontrado" });
            }
            return Ok(jogo);
        }

        [HttpGet("top-avaliados")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTop10MelhorAvaliados()
        {
            var jogos = await _jogoServices.ListarTop10JogosMelhorAvaliados();
            return Ok(jogos);
        }
    }
}
