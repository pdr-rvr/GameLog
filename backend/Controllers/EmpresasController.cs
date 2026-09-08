using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly EmpresaServices _empresaServices;

        public EmpresasController(EmpresaServices empresaServices)
        {
            _empresaServices = empresaServices;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListarTodasEmpresas()
        {
            try
            {
                var empresas = await _empresaServices.ListarEmpresas();
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar empresas: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEmpresaPorId(int id)
        {
            try
            {
                var empresa = await _empresaServices.ObterEmpresaPorId(id);
                if (empresa == null)
                {
                    return NotFound(new { message = "Empresa não encontrada" });
                }

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter empresa: " + ex.Message });
            }
        }

        [HttpGet("{id}/jogos")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarJogosPorEmpresa(int id)
        {
            try
            {
                var jogos = await _empresaServices.ListarJogosPorEmpresa(id);
                return Ok(jogos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar jogos da empresa: " + ex.Message });
            }
        }
    }
}
