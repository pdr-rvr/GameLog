using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ListarTodosJogos()
        {
            try
            {
                var jogos = _jogoServices.ListarJogos();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("top-3-avaliados")]
        public IActionResult ListarTop3MelhorAvaliados()
        {
            try
            {
                var jogos = _jogoServices.ListarTop3JogosMelhorAvaliados();
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}