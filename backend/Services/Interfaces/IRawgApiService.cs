using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IRawgApiService
    {
        Task<List<RawgGameItemDTO>> ObterJogosPopularesRawg(int pagina, int pageSize = 40);
        Task<List<RawgGameItemDTO>> ObterJogosRawgQueryAsync(string queryParams, int pagina, int pageSize = 40);
        Task<RawgSearchResultDTO> BuscarJogosExternos(string termo, int pagina = 1, int itensPorPagina = 20);
        Task<List<JogoDTO>> BuscarJogosExternosFormatados(string? busca, int? ano, string? genero, string? empresa, int maxItens = 40);
        Task<RawgGameDetailDTO?> ObterDetalhesJogoExterno(int rawgId);
        Task<JogoDTO?> ImportarJogoRawgParaBanco(int rawgId);
    }
}
