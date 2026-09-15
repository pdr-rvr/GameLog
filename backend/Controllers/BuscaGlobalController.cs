using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    [EnableRateLimiting("ExternalApiLimiter")]
    public class BuscaGlobalController : ControllerBase
    {
        private readonly IBuscaGlobalService _buscaGlobalService;

        public BuscaGlobalController(IBuscaGlobalService buscaGlobalService)
        {
            _buscaGlobalService = buscaGlobalService;
        }

        /// <summary>
        /// Realiza busca global unificada em Jogos (locais + externos transparentes), Usuários e Listas Públicas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Buscar([FromQuery] string? q, [FromQuery] int limite = 5)
        {
            var resultado = await _buscaGlobalService.BuscarAsync(q, limite);
            return Ok(resultado);
        }
    }
}
