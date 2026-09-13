using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<IEnumerable<UsuarioDTO>> ListarUsuarios();
        Task<UsuarioDTO?> ObterUsuarioPorId(Guid id);
        Task<UsuarioDTO?> EditarUsuario(Guid id, string senhaAtual, EditarUsuarioDTO usuarioDTO);
        Task<bool> DeletarUsuario(Guid id, string senhaAtual);
        Task<bool> EmailEmUso(string email);
        Task<bool> NomeUsuarioEmUso(string nomeUsuario);
        Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5);
        Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId);
    }
}
