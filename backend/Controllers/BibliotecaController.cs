using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
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

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst("id")?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(claim, out var id))
            {
                return id;
            }
            return null;
        }

        /// <summary>
        /// Adiciona ou atualiza um jogo na biblioteca do usuário autenticado.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SalvarItem([FromBody] SalvarItemBibliotecaDTO dto)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var item = await _bibliotecaServices.SalvarItemBiblioteca(usuarioId.Value, dto);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao salvar na biblioteca.", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém o status de um jogo na biblioteca do usuário autenticado.
        /// </summary>
        [HttpGet("jogo/{jogoId}")]
        [Authorize]
        public async Task<IActionResult> ObterStatusJogo(int jogoId)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            var item = await _bibliotecaServices.ObterStatusJogo(usuarioId.Value, jogoId);
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
        public async Task<IActionResult> RemoverItem(int jogoId)
        {
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            var sucesso = await _bibliotecaServices.RemoverDaBiblioteca(usuarioId.Value, jogoId);
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
        public async Task<IActionResult> ListarBibliotecaUsuario(int usuarioId, [FromQuery] StatusJogo? status, [FromQuery] string? busca)
        {
            var itens = await _bibliotecaServices.ListarBibliotecaUsuario(usuarioId, status, busca);
            return Ok(itens);
        }

        /// <summary>
        /// Obtém contadores de status da biblioteca de um usuário.
        /// </summary>
        [HttpGet("usuario/{usuarioId}/estatisticas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEstatisticas(int usuarioId)
        {
            var stats = await _bibliotecaServices.ObterEstatisticasBiblioteca(usuarioId);
            return Ok(stats);
        }

        /// <summary>
        /// Obtém o Top 5 de jogos favoritos de um usuário.
        /// </summary>
        [HttpGet("usuario/{usuarioId}/favoritos")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterFavoritos(int usuarioId)
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
            var usuarioId = GetCurrentUserId();
            if (!usuarioId.HasValue) return Unauthorized(new { message = "Usuário não autenticado." });

            var atualizados = await _bibliotecaServices.SalvarJogosFavoritos(usuarioId.Value, dto);
            return Ok(atualizados);
        }
    }
}
