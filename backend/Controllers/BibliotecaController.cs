using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Extensions;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BibliotecaController : ControllerBase
    {
        private readonly BibliotecaServices _bibliotecaServices;

        public BibliotecaController(BibliotecaServices bibliotecaServices)
        {
            _bibliotecaServices = bibliotecaServices;
        }

        /// <summary>
        /// Adiciona ou atualiza um jogo na biblioteca do usuário autenticado.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SalvarItem([FromBody] SalvarItemBibliotecaDTO dto)
        {
            var usuarioId = User.GetUserId();
            var item = await _bibliotecaServices.SalvarItemBiblioteca(usuarioId, dto);
            return Ok(item);
        }

        /// <summary>
        /// Obtém o status de um jogo na biblioteca do usuário autenticado.
        /// </summary>
        [HttpGet("jogo/{jogoId}")]
        [Authorize]
        public async Task<IActionResult> ObterStatusJogo(Guid jogoId)
        {
            var usuarioId = User.GetUserId();
            var item = await _bibliotecaServices.ObterStatusJogo(usuarioId, jogoId);
            if (item == null)
            {
                return Ok(new { naBiblioteca = false });
            }

            return Ok(new { naBiblioteca = true, item });
        }

        /// <summary>
        /// Remove um jogo da biblioteca do usuário autenticado.
        /// </summary>
        [HttpDelete("jogo/{jogoId}")]
        [Authorize]
        public async Task<IActionResult> RemoverItem(Guid jogoId)
        {
            var usuarioId = User.GetUserId();
            var sucesso = await _bibliotecaServices.RemoverDaBiblioteca(usuarioId, jogoId);
            if (!sucesso)
            {
                return NotFound(new { message = "Jogo não encontrado na sua biblioteca." });
            }

            return Ok(new { message = "Jogo removido da biblioteca com sucesso." });
        }

        /// <summary>
        /// Lista a biblioteca de jogos de um usuário com filtros opcionais.
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarBibliotecaUsuario(Guid usuarioId, [FromQuery] StatusJogo? status, [FromQuery] string? busca)
        {
            var itens = await _bibliotecaServices.ListarBibliotecaUsuario(usuarioId, status, busca);
            return Ok(itens);
        }

        /// <summary>
        /// Obtém contadores de status da biblioteca de um usuário.
        /// </summary>
        [HttpGet("usuario/{usuarioId}/estatisticas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEstatisticas(Guid usuarioId)
        {
            var stats = await _bibliotecaServices.ObterEstatisticasBiblioteca(usuarioId);
            return Ok(stats);
        }

        /// <summary>
        /// Obtém o Top 5 de jogos favoritos de um usuário.
        /// </summary>
        [HttpGet("usuario/{usuarioId}/favoritos")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterFavoritos(Guid usuarioId)
        {
            var favoritos = await _bibliotecaServices.ObterJogosFavoritos(usuarioId);
            return Ok(favoritos);
        }

        /// <summary>
        /// Salva ou atualiza os 5 jogos favoritos do usuário autenticado.
        /// </summary>
        [HttpPut("meus-favoritos")]
        [Authorize]
        public async Task<IActionResult> SalvarMeusFavoritos([FromBody] SalvarJogosFavoritosDTO dto)
        {
            var usuarioId = User.GetUserId();
            var atualizados = await _bibliotecaServices.SalvarJogosFavoritos(usuarioId, dto);
            return Ok(atualizados);
        }
    }
}
