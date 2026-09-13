using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IFeedService
    {
        Task<List<ItemFeedSocialDTO>> ObterFeedSocial(Guid usuarioId, int pagina = 1, int itensPorPagina = 20);
        Task<List<ItemAtividadeTimelineDTO>> ObterTimelineAtividades(Guid usuarioId, int pagina = 1, int itensPorPagina = 30);
    }
}
