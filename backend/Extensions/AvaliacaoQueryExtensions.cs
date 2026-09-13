using System;
using System.Linq;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Extensions
{
    public static class AvaliacaoQueryExtensions
    {
        public static IQueryable<AvaliacaoDTO> ProjetarParaDTO(this IQueryable<Avaliacao> query, Guid? usuarioId = null)
        {
            return query.Select(a => new AvaliacaoDTO
            {
                AvaliacaoId = a.Id,
                Nota = a.Nota,
                JogoId = a.Jogo.Id,
                NomeJogo = a.Jogo.Titulo,
                ImagemJogo = a.Jogo.Imagem,
                NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                DataLancamentoJogo = a.Jogo.DataLancamento,
                UsuarioId = a.Usuario.Id,
                NomeUsuario = a.Usuario.NomeUsuario,
                FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                TextoAvaliacao = a.TextoAvaliacao,
                DataPublicacao = a.DataPublicacao,
                TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                CurtidaPorMim = usuarioId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida),
                TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
            });
        }
    }
}
