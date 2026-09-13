using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IEmpresaService
    {
        Task<IEnumerable<EmpresaDTO>> ListarEmpresas();
        Task<EmpresaDTO?> ObterEmpresaPorId(Guid id);
        Task<IEnumerable<JogoDTO>> ListarJogosPorEmpresa(Guid empresaId);
    }
}
