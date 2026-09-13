using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IListaService
    {
        Task<ListaDeJogosDTO> CriarLista(Guid usuarioId, CriarListaDTO dto);
        Task<ListaDeJogosDTO?> ObterListaPorId(Guid listaId, Guid? usuarioAutenticadoId = null);
        Task<List<ListaDeJogosDTO>> ListarListasDoUsuario(Guid usuarioId, Guid? usuarioAutenticadoId = null);
        Task<ListaDeJogosDTO?> EditarLista(Guid listaId, Guid usuarioId, EditarListaDTO dto);
        Task<bool> DeletarLista(Guid listaId, Guid usuarioId);
        Task<bool> AdicionarJogoNaLista(Guid listaId, Guid usuarioId, Guid jogoId);
        Task<bool> RemoverJogoDaLista(Guid listaId, Guid usuarioId, Guid jogoId);
    }
}
