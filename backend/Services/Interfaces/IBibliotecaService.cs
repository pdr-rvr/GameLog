using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IBibliotecaService
    {
        Task<ItemBibliotecaDTO> SalvarItemBiblioteca(Guid usuarioId, SalvarItemBibliotecaDTO dto);
        Task<ItemBibliotecaDTO?> ObterStatusJogo(Guid usuarioId, Guid jogoId);
        Task<bool> RemoverDaBiblioteca(Guid usuarioId, Guid jogoId);
        Task<List<ItemBibliotecaDTO>> ListarBibliotecaUsuario(Guid usuarioId, StatusJogo? statusFiltro = null, string? busca = null);
        Task<EstatisticasBibliotecaDTO> ObterEstatisticasBiblioteca(Guid usuarioId);
        Task<List<JogoFavoritoDTO>> ObterJogosFavoritos(Guid usuarioId);
        Task<List<JogoFavoritoDTO>> SalvarJogosFavoritos(Guid usuarioId, SalvarJogosFavoritosDTO dto);
    }
}
