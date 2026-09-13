using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO);
        Task<IEnumerable<UsuarioDTO>> ListarUsuarios();
        Task<UsuarioDTO?> ObterUsuarioPorId(Guid id);
        Task<UsuarioDTO> CriarUsuario(CriarUsuarioDTO usuarioDTO);
        Task<UsuarioDTO?> EditarUsuario(Guid id, string senhaAtual, EditarUsuarioDTO usuarioDTO);
        Task<bool> DeletarUsuario(Guid id, string senhaAtual);
        Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5);
        Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId);
        Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(Guid seguidorId, Guid seguidoId);
        Task<bool> VerificarSeSegue(Guid seguidorId, Guid seguidoId);
        Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(Guid usuarioId, Guid? solicitanteId = null);
        Task<List<UsuarioConexaoDTO>> ObterSeguidores(Guid usuarioId, Guid? solicitanteId = null);
        Task<List<UsuarioConexaoDTO>> ObterSeguindo(Guid usuarioId, Guid? solicitanteId = null);
        Task<List<ItemFeedSocialDTO>> ObterFeedSocial(Guid usuarioId, int pagina = 1, int itensPorPagina = 20);
        Task<List<ItemAtividadeTimelineDTO>> ObterTimelineAtividades(Guid usuarioId, int pagina = 1, int itensPorPagina = 30);
        Task<bool> EmailEmUso(string email);
        Task<bool> NomeUsuarioEmUso(string nomeUsuario);
    }
}
