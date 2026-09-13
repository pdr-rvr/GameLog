using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IAvaliacaoService
    {
        Task<AvaliacaoDTO> CriarAvaliacao(CriarAvaliacaoDTO avaliacaoDTO, Guid usuarioId);
        Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoes(Guid? usuarioId = null, int? notaExata = null, string? ordenacao = "recentes", int? pagina = null, int? itensPorPagina = null);
        Task<AvaliacaoDTO?> ObterAvaliacaoPorId(Guid id, Guid? usuarioId = null);
        Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorJogo(Guid jogoId, Guid? usuarioId = null);
        Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorUsuario(Guid usuarioId, Guid? solicitanteId = null);
        Task<AvaliacaoDTO?> EditarAvaliacao(Guid id, EditarAvaliacaoDTO avaliacaoDTO, Guid usuarioId);
        Task<bool> DeletarAvaliacao(Guid id, Guid usuarioId);
        Task<(bool Curtido, int TotalCurtidas)> AlternarCurtida(Guid avaliacaoId, Guid usuarioId);
        Task<bool> RemoverCurtida(Guid avaliacaoId, Guid usuarioId);
        Task<int> ContarCurtidas(Guid avaliacaoId);
        Task<bool> UsuarioCurtiu(Guid avaliacaoId, Guid usuarioId);
        Task<RespostaDeAvaliacaoDTO> AdicionarResposta(Guid avaliacaoId, Guid usuarioId, CriarRespostaDTO dto);
        Task<IEnumerable<RespostaDeAvaliacaoDTO>> ListarRespostasPorAvaliacao(Guid avaliacaoId, Guid? usuarioId = null);
        Task<bool> DeletarResposta(Guid respostaId, Guid usuarioId);
        Task<(bool Curtido, int TotalCurtidas)> AlternarCurtidaResposta(Guid respostaId, Guid usuarioId);
    }
}
