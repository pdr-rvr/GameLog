using System;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Extensions;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class ComunidadeServices : IComunidadeService
    {
        private readonly GameLogContext _context;

        public ComunidadeServices(GameLogContext context)
        {
            _context = context;
        }

        public async Task<TendenciasComunidadeDTO> ObterTendencias(Guid? usuarioId)
        {
            // 1. Jogos Mais Discutidos
            var jogosStats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    TotalAvaliacoes = g.Count(),
                    Media = g.Average(a => (double)a.Nota)
                })
                .OrderByDescending(g => g.TotalAvaliacoes)
                .ThenByDescending(g => g.Media)
                .Take(5)
                .ToListAsync();

            var topJogoIds = jogosStats.Select(s => s.JogoId).ToList();
            var jogosEntidades = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .Where(j => topJogoIds.Contains(j.Id))
                .ToListAsync();

            var jogosMaisDiscutidos = jogosStats.Select(s =>
            {
                var j = jogosEntidades.FirstOrDefault(x => x.Id == s.JogoId);
                if (j == null) return null;
                return new JogoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    EmpresaId = j.Empresa?.Id ?? Guid.Empty,
                    NomeEmpresa = j.Empresa?.NomeEmpresa ?? string.Empty,
                    PublicadoraId = j.Publicadora?.Id,
                    NomePublicadora = j.Publicadora?.NomeEmpresa,
                    EstaAtivo = j.EstaAtivo,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = Math.Round(s.Media, 1),
                    TotalAvaliacoes = s.TotalAvaliacoes,
                    EhExterno = false
                };
            }).Where(j => j != null).Select(j => j!).ToList();

            // 2. Avaliacoes Mais Curtidas / Relevantes
            var avaliacoesMaisCurtidas = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.Usuario)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Empresa)
                .Include(a => a.CurtidasDeAvaliacao)
                .Include(a => a.RespostasDeAvaliacao)
                .Where(a => a.EstaAtivo && a.Jogo.EstaAtivo && a.Usuario.EstaAtivo)
                .OrderByDescending(a => a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida))
                .ThenByDescending(a => a.DataPublicacao)
                .Take(6)
                .ProjetarParaDTO(usuarioId)
                .ToListAsync();

            // 3. Listas / Colecoes em Destaque
            var listas = await _context.ListasDeJogos
                .AsNoTracking()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                .Where(l => l.EstaAtivo && l.EstaPublica && l.Itens.Any(i => i.EstaAtivo))
                .OrderByDescending(l => l.Itens.Count(i => i.EstaAtivo))
                .ThenByDescending(l => l.DataAtualizacao)
                .Take(6)
                .Select(l => new ListaDeJogosDTO
                {
                    ListaId = l.Id,
                    UsuarioId = l.Usuario.Id,
                    NomeUsuario = l.Usuario.NomeUsuario,
                    FotoPerfilUsuario = l.Usuario.FotoDePerfil,
                    Titulo = l.Titulo,
                    Descricao = l.Descricao,
                    EstaPublica = l.EstaPublica,
                    DataCriacao = l.DataCriacao,
                    DataAtualizacao = l.DataAtualizacao,
                    TotalJogos = l.Itens.Count(i => i.EstaAtivo),
                    CapasPreview = l.Itens
                        .Where(i => i.EstaAtivo && i.Jogo != null && !string.IsNullOrEmpty(i.Jogo.Imagem))
                        .OrderBy(i => i.Ordem)
                        .Select(i => i.Jogo.Imagem)
                        .Take(4)
                        .ToList()
                })
                .ToListAsync();

            var totalAvaliacoes = await _context.Avaliacoes.CountAsync(a => a.EstaAtivo);
            var totalUsuarios = await _context.Usuarios.CountAsync(u => u.EstaAtivo);

            return new TendenciasComunidadeDTO
            {
                JogosMaisDiscutidos = jogosMaisDiscutidos,
                AvaliacoesMaisCurtidas = avaliacoesMaisCurtidas,
                ListasEmDestaque = listas,
                TotalAvaliacoesPlataforma = totalAvaliacoes,
                TotalJogadoresAtivos = totalUsuarios
            };
        }
    }
}
