using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IRecomendacaoService
    {
        Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5);
        Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId);
    }
}
