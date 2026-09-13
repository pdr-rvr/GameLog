using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IJogoService
    {
        Task<IEnumerable<JogoDTO>> ListarJogos();
        Task<PagedResult<JogoDTO>> ListarJogosPaginados(
            int pagina = 1,
            int itensPorPagina = 12,
            string? busca = null,
            IEnumerable<string>? generos = null,
            int? ano = null,
            string? empresa = null,
            double? notaMinima = null,
            string ordenacao = "melhores");
        Task<JogoDTO?> ObterJogoPorId(Guid id);
        Task<IEnumerable<JogoDTO>> ListarTop10JogosMelhorAvaliados();
        Task<IEnumerable<JogoDTO>> ListarDestaquesHeroAsync(int limite = 5);
        Task<MetadadosFiltrosDTO> ObterMetadadosFiltros();
    }
}
