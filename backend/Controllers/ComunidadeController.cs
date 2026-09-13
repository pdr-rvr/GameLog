using System;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Extensions;
using GameLog_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ComunidadeController : ControllerBase
    {
        private readonly IComunidadeService _comunidadeService;

        public ComunidadeController(IComunidadeService comunidadeService)
        {
            _comunidadeService = comunidadeService;
        }

        [HttpGet("tendencias")]
        public async Task<IActionResult> ObterTendencias()
        {
            var usuarioId = User.GetUserIdOrNull();
            var tendencias = await _comunidadeService.ObterTendencias(usuarioId);
            return Ok(tendencias);
        }
    }
}
