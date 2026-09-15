using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class FeedService : IFeedService
    {
        private readonly GameLogContext _context;

        public FeedService(GameLogContext context)
        {
            _context = context;
        }

        public async Task<List<ItemFeedSocialDTO>> ObterFeedSocial(Guid usuarioId, int pagina = 1, int itensPorPagina = 20)
        {
            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 50);

            var seguindoIds = await _context.SegueUsuarios
                .AsNoTracking()
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo)
                .Select(s => s.UsuarioSeguido.Id)
                .ToListAsync();

            if (!seguindoIds.Any())
            {
                return new List<ItemFeedSocialDTO>();
            }

            // 1. Avaliações postadas pelos seguidos
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.Usuario)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Empresa)
                .Where(a => seguindoIds.Contains(a.Usuario.Id) && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Take(itensPorPagina * 2)
                .Select(a => new ItemFeedSocialDTO
                {
                    Id = "eval-" + a.Id,
                    TipoAtividade = "Avaliacao",
                    DataAtividade = a.DataPublicacao,
                    AutorId = a.Usuario.Id,
                    AutorNome = a.Usuario.NomeUsuario,
                    AutorFoto = a.Usuario.FotoDePerfil,
                    JogoId = a.Jogo.Id,
                    JogoTitulo = a.Jogo.Titulo,
                    JogoImagem = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    TextoAvaliacao = a.TextoAvaliacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .ToListAsync();

            // 2. Comentários / Discussões feitos por seguidos em análises
            var comentarios = await _context.RespostasDeAvaliacao
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Avaliacao)
                    .ThenInclude(a => a!.Usuario)
                .Include(r => r.Avaliacao)
                    .ThenInclude(a => a!.Jogo)
                        .ThenInclude(j => j.Empresa)
                .Include(r => r.CurtidasDeRespostas)
                .Where(r => r.UsuarioId.HasValue && seguindoIds.Contains(r.UsuarioId.Value) && r.EstaAtivo && r.Avaliacao != null && r.Avaliacao.EstaAtivo && r.Avaliacao.Jogo != null && r.Avaliacao.Usuario != null)
                .OrderByDescending(r => r.DataCriacao)
                .Take(itensPorPagina * 2)
                .Select(r => new ItemFeedSocialDTO
                {
                    Id = "comm-" + r.Id,
                    TipoAtividade = "Discussao",
                    DataAtividade = r.DataCriacao,
                    AutorId = r.UsuarioId!.Value,
                    AutorNome = r.Usuario != null ? r.Usuario.NomeUsuario : "Gamer",
                    AutorFoto = r.Usuario != null ? r.Usuario.FotoDePerfil : null,
                    JogoId = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Id : Guid.Empty,
                    JogoTitulo = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Titulo : "Jogo",
                    JogoImagem = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Imagem : null,
                    NomeEmpresa = r.Avaliacao != null && r.Avaliacao.Jogo != null && r.Avaliacao.Jogo.Empresa != null ? r.Avaliacao.Jogo.Empresa.NomeEmpresa : null,
                    AvaliacaoId = r.AvaliacaoId,
                    ComentarioTexto = r.Comentario,
                    AutorAvaliacaoRespondidaId = r.Avaliacao != null && r.Avaliacao.Usuario != null ? r.Avaliacao.Usuario.Id : Guid.Empty,
                    AutorAvaliacaoRespondidaNome = r.Avaliacao != null && r.Avaliacao.Usuario != null ? r.Avaliacao.Usuario.NomeUsuario : "Gamer",
                    AvaliacaoOriginalTexto = r.Avaliacao != null ? r.Avaliacao.TextoAvaliacao : null,
                    TotalCurtidas = r.CurtidasDeRespostas.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = r.CurtidasDeRespostas.Any(c => c.UsuarioId == usuarioId && c.EstaAtivo && c.Curtida)
                })
                .ToListAsync();

            // 3. Listas públicas criadas pelos seguidos
            var listas = await _context.ListasDeJogos
                .AsNoTracking()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                .Where(l => seguindoIds.Contains(l.UsuarioId) && l.EstaPublica && l.EstaAtivo)
                .OrderByDescending(l => l.DataCriacao)
                .Take(itensPorPagina * 2)
                .ToListAsync();

            var listasItems = listas.Select(l => new ItemFeedSocialDTO
            {
                Id = "lista-" + l.Id,
                TipoAtividade = "ListaCriada",
                DataAtividade = l.DataCriacao,
                AutorId = l.UsuarioId,
                AutorNome = l.Usuario != null ? l.Usuario.NomeUsuario : "Gamer",
                AutorFoto = l.Usuario != null ? l.Usuario.FotoDePerfil : null,
                ListaId = l.Id,
                ListaTitulo = l.Titulo,
                ListaDescricao = l.Descricao,
                TotalJogosLista = l.Itens.Count(i => i.EstaAtivo),
                CapasPreviewLista = l.Itens
                    .Where(i => i.EstaAtivo && i.Jogo != null && !string.IsNullOrEmpty(i.Jogo.Imagem))
                    .OrderBy(i => i.Ordem)
                    .Select(i => i.Jogo!.Imagem)
                    .Take(4)
                    .ToList()
            }).ToList();

            return avaliacoes
                .Concat(comentarios)
                .Concat(listasItems)
                .OrderByDescending(item => item.DataAtividade)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();
        }

        public async Task<List<ItemAtividadeTimelineDTO>> ObterTimelineAtividades(Guid usuarioId, int pagina = 1, int itensPorPagina = 30)
        {
            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 60);

            var seguindoIds = await _context.SegueUsuarios
                .AsNoTracking()
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo)
                .Select(s => s.UsuarioSeguido.Id)
                .ToListAsync();

            if (!seguindoIds.Any())
            {
                return new List<ItemAtividadeTimelineDTO>();
            }

            // 1. Avaliações
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.Usuario)
                .Include(a => a.Jogo)
                .Where(a => seguindoIds.Contains(a.Usuario.Id) && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Take(itensPorPagina * 2)
                .Select(a => new ItemAtividadeTimelineDTO
                {
                    Id = "act-eval-" + a.Id,
                    Tipo = "Avaliou",
                    DataAtividade = a.DataPublicacao,
                    UsuarioId = a.Usuario.Id,
                    UsuarioNome = a.Usuario.NomeUsuario,
                    UsuarioFoto = a.Usuario.FotoDePerfil,
                    JogoId = a.Jogo.Id,
                    JogoTitulo = a.Jogo.Titulo,
                    JogoImagem = a.Jogo.Imagem,
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    TextoCurto = !string.IsNullOrEmpty(a.TextoAvaliacao) 
                        ? (a.TextoAvaliacao.Length > 80 ? a.TextoAvaliacao.Substring(0, 80) + "..." : a.TextoAvaliacao) 
                        : null
                })
                .ToListAsync();

            // 2. Status na Biblioteca
            var bibItems = await _context.ItensBiblioteca
                .AsNoTracking()
                .Include(b => b.Jogo)
                .Where(b => seguindoIds.Contains(b.UsuarioId) && b.EstaAtivo)
                .OrderByDescending(b => b.DataAtualizacao)
                .Take(itensPorPagina * 2)
                .ToListAsync();

            var bibUserIds = bibItems.Select(b => b.UsuarioId).Distinct().ToList();
            var usuariosMap = await _context.Usuarios
                .AsNoTracking()
                .Where(u => bibUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.NomeUsuario, u.FotoDePerfil });

            var statusItems = bibItems.Select(b =>
            {
                usuariosMap.TryGetValue(b.UsuarioId, out var u);
                string tipo;
                if (b.Status == StatusJogo.Zerado) tipo = "Zerou";
                else if (b.Status == StatusJogo.Jogando) tipo = "Jogando";
                else tipo = "AdicionouBiblioteca";

                return new ItemAtividadeTimelineDTO
                {
                    Id = "act-bib-" + b.Id,
                    Tipo = tipo,
                    StatusBiblioteca = b.Status.ToString(),
                    DataAtividade = b.DataConclusao ?? b.DataAtualizacao,
                    UsuarioId = b.UsuarioId,
                    UsuarioNome = u?.NomeUsuario ?? "Gamer",
                    UsuarioFoto = u?.FotoDePerfil,
                    JogoId = b.JogoId,
                    JogoTitulo = b.Jogo?.Titulo ?? "Jogo",
                    JogoImagem = b.Jogo?.Imagem
                };
            }).ToList();

            // 3. Comentários / Discussões
            var comentarios = await _context.RespostasDeAvaliacao
                .AsNoTracking()
                .Include(r => r.Usuario)
                .Include(r => r.Avaliacao)
                    .ThenInclude(a => a!.Usuario)
                .Include(r => r.Avaliacao)
                    .ThenInclude(a => a!.Jogo)
                .Where(r => r.UsuarioId.HasValue && seguindoIds.Contains(r.UsuarioId.Value) && r.EstaAtivo && r.Avaliacao != null && r.Avaliacao.EstaAtivo && r.Avaliacao.Jogo != null && r.Avaliacao.Usuario != null)
                .OrderByDescending(r => r.DataCriacao)
                .Take(itensPorPagina * 2)
                .Select(r => new ItemAtividadeTimelineDTO
                {
                    Id = "act-comm-" + r.Id,
                    Tipo = "Comentou",
                    DataAtividade = r.DataCriacao,
                    UsuarioId = r.UsuarioId!.Value,
                    UsuarioNome = r.Usuario != null ? r.Usuario.NomeUsuario : "Gamer",
                    UsuarioFoto = r.Usuario != null ? r.Usuario.FotoDePerfil : null,
                    JogoId = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Id : Guid.Empty,
                    JogoTitulo = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Titulo : "Jogo",
                    JogoImagem = r.Avaliacao != null && r.Avaliacao.Jogo != null ? r.Avaliacao.Jogo.Imagem : null,
                    AvaliacaoId = r.AvaliacaoId,
                    AutorAvaliacaoRespondidaNome = r.Avaliacao != null && r.Avaliacao.Usuario != null ? r.Avaliacao.Usuario.NomeUsuario : "Gamer",
                    ComentarioTexto = r.Comentario,
                    TextoCurto = !string.IsNullOrEmpty(r.Comentario)
                        ? (r.Comentario.Length > 80 ? r.Comentario.Substring(0, 80) + "..." : r.Comentario)
                        : null
                })
                .ToListAsync();

            // 4. Criação de Listas
            var listas = await _context.ListasDeJogos
                .AsNoTracking()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                .Where(l => seguindoIds.Contains(l.UsuarioId) && l.EstaPublica && l.EstaAtivo)
                .OrderByDescending(l => l.DataCriacao)
                .Take(itensPorPagina * 2)
                .Select(l => new ItemAtividadeTimelineDTO
                {
                    Id = "act-list-" + l.Id,
                    Tipo = "CriouLista",
                    DataAtividade = l.DataCriacao,
                    UsuarioId = l.UsuarioId,
                    UsuarioNome = l.Usuario != null ? l.Usuario.NomeUsuario : "Gamer",
                    UsuarioFoto = l.Usuario != null ? l.Usuario.FotoDePerfil : null,
                    ListaId = l.Id,
                    ListaTitulo = l.Titulo,
                    TotalJogos = l.Itens.Count(i => i.EstaAtivo)
                })
                .ToListAsync();

            // 5. Adições de Jogos a Listas
            var itensLista = await _context.ItensDeListas
                .AsNoTracking()
                .Include(i => i.ListaDeJogos)
                    .ThenInclude(l => l!.Usuario)
                .Include(i => i.Jogo)
                .Where(i => i.EstaAtivo && i.ListaDeJogos != null && i.ListaDeJogos.EstaPublica && i.ListaDeJogos.EstaAtivo && i.ListaDeJogos.Usuario != null && i.Jogo != null && seguindoIds.Contains(i.ListaDeJogos.UsuarioId))
                .OrderByDescending(i => i.DataAdicionado)
                .Take(itensPorPagina * 2)
                .Select(i => new ItemAtividadeTimelineDTO
                {
                    Id = "act-listitem-" + i.Id,
                    Tipo = "AdicionouJogoLista",
                    DataAtividade = i.DataAdicionado,
                    UsuarioId = i.ListaDeJogos != null ? i.ListaDeJogos.UsuarioId : Guid.Empty,
                    UsuarioNome = i.ListaDeJogos != null && i.ListaDeJogos.Usuario != null ? i.ListaDeJogos.Usuario.NomeUsuario : "Gamer",
                    UsuarioFoto = i.ListaDeJogos != null && i.ListaDeJogos.Usuario != null ? i.ListaDeJogos.Usuario.FotoDePerfil : null,
                    JogoId = i.Jogo != null ? i.Jogo.Id : Guid.Empty,
                    JogoTitulo = i.Jogo != null ? i.Jogo.Titulo : "Jogo",
                    JogoImagem = i.Jogo != null ? i.Jogo.Imagem : null,
                    ListaId = i.ListaDeJogosId,
                    ListaTitulo = i.ListaDeJogos != null ? i.ListaDeJogos.Titulo : "Lista"
                })
                .ToListAsync();

            return avaliacoes
                .Concat(statusItems)
                .Concat(comentarios)
                .Concat(listas)
                .Concat(itensLista)
                .OrderByDescending(item => item.DataAtividade)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();
        }
    }
}
