using System;
using System.Threading.Tasks;
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
            var empresas = await _empresaServices.ListarEmpresas();
            return Ok(empresas);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterEmpresaPorId(Guid id)
        {
            var empresa = await _empresaServices.ObterEmpresaPorId(id);
            if (empresa == null)
            {
                return NotFound(new { message = "Empresa não encontrada" });
            }

            return Ok(empresa);
        }

        [HttpGet("{id}/jogos")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarJogosPorEmpresa(Guid id)
        {
            var jogos = await _empresaServices.ListarJogosPorEmpresa(id);
            return Ok(jogos);
        }
    }
}
