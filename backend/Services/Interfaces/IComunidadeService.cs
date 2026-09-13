using System;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IComunidadeService
    {
        Task<TendenciasComunidadeDTO> ObterTendencias(Guid? usuarioId);
    }
}
