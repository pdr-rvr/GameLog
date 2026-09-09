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
        private readonly RawgApiService _rawgApiService;

        public JogosController(JogoServices jogoServices, RawgApiService rawgApiService)
        {
            _jogoServices = jogoServices;
            _rawgApiService = rawgApiService;
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

        [HttpGet("metadados-filtros")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterMetadadosFiltros()
        {
            var metadados = await _jogoServices.ObterMetadadosFiltros();
            return Ok(metadados);
        }

        /// <summary>
        /// Busca jogos em tempo real na base de dados global da RAWG (+500k jogos).
        /// </summary>
        [HttpGet("rawg/buscar")]
        [AllowAnonymous]
        public async Task<IActionResult> BuscarNaRawg(
            [FromQuery] string termo,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 20)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return BadRequest(new { message = "O termo de busca é obrigatório." });
            }

            var resultado = await _rawgApiService.BuscarJogosExternos(termo, pagina, itensPorPagina);
            return Ok(resultado);
        }

        /// <summary>
        /// Importa sob demanda um jogo da RAWG para a base de dados local do GameLog.
        /// </summary>
        [HttpPost("rawg/importar/{rawgId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportarJogoRawg(int rawgId)
        {
            if (rawgId <= 0)
            {
                return BadRequest(new { message = "ID RAWG inválido." });
            }

            var jogoImportado = await _rawgApiService.ImportarJogoRawgParaBanco(rawgId);
            if (jogoImportado == null)
            {
                return NotFound(new { message = "Jogo não encontrado na RAWG ou falha na importação." });
            }

            return Ok(jogoImportado);
        }
    }
}
