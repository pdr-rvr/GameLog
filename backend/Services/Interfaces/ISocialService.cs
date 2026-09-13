using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface ISocialService
    {
        Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(Guid seguidorId, Guid seguidoId);
        Task<bool> VerificarSeSegue(Guid seguidorId, Guid seguidoId);
        Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(Guid usuarioId, Guid? solicitanteId = null);
        Task<List<UsuarioConexaoDTO>> ObterSeguidores(Guid usuarioId, Guid? solicitanteId = null);
        Task<List<UsuarioConexaoDTO>> ObterSeguindo(Guid usuarioId, Guid? solicitanteId = null);
    }
}
