using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GameLog_Backend.Services
{
    public class RecomendacaoService : IRecomendacaoService
    {
        private readonly GameLogContext _context;
        private readonly IMemoryCache _cache;

        public RecomendacaoService(GameLogContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5)
        {
            var cacheKey = $"fav_genres_{id}_{topN}";
            if (_cache.TryGetValue<List<GeneroFavoritoDTO>>(cacheKey, out var cachedGenres) && cachedGenres != null)
            {
                return cachedGenres;
            }

            var favoritos = await _context.JogosFavoritosUsuarios
                .AsNoTracking()
                .Where(f => f.Usuario.Id == id && f.EstaAtivo)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == id && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var biblioteca = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == id && b.EstaAtivo)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var pontuacaoGeneros = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var fav in favoritos.Where(f => f.Jogo?.Generos != null))
            {
                foreach (var g in fav.Jogo.Generos)
                {
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + 15.0;
                }
            }

            foreach (var av in avaliacoes.Where(a => a.Jogo?.Generos != null))
            {
                foreach (var g in av.Jogo.Generos)
                {
                    double peso = av.Nota >= 4 ? 2.5 : (av.Nota == 3 ? 1.0 : -1.0);
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + (av.Nota * peso);
                }
            }

            foreach (var bib in biblioteca.Where(b => b.Jogo?.Generos != null))
            {
                foreach (var g in bib.Jogo.Generos)
                {
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + 3.0;
                }
            }

            var topGeneros = pontuacaoGeneros
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .Select(kv => new GeneroFavoritoDTO { Genero = kv.Key })
                .ToList();

            _cache.Set(cacheKey, topGeneros, TimeSpan.FromMinutes(10));
            return topGeneros;
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId)
        {
            var cacheKey = $"rec_user_{usuarioId}";
            if (_cache.TryGetValue<List<JogoRecomendacaoDTO>>(cacheKey, out var cachedRecs) && cachedRecs != null)
            {
                return cachedRecs;
            }

            var jogosAvaliadosIds = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Select(a => a.Jogo.Id)
                .ToListAsync();

            var jogosFavoritos = await _context.JogosFavoritosUsuarios
                .AsNoTracking()
                .Where(f => f.Usuario.Id == usuarioId && f.EstaAtivo)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            var jogosFavoritosIds = jogosFavoritos.Select(f => f.Jogo.Id).ToList();

            var bibliotecaIds = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == usuarioId && b.EstaAtivo && b.Status != StatusJogo.QueroJogar)
                .Select(b => b.Jogo.Id)
                .ToListAsync();

            var jogosExcluidos = new HashSet<Guid>(jogosAvaliadosIds.Concat(jogosFavoritosIds).Concat(bibliotecaIds));

            var afinidadeGeneros = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var afinidadeEstudios = new Dictionary<Guid, double>();
            var nomesEstudiosFavoritos = new Dictionary<Guid, string>();

            foreach (var fav in jogosFavoritos.Where(f => f.Jogo != null))
            {
                if (fav.Jogo.Generos != null)
                {
                    foreach (var g in fav.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + 15.0;
                    }
                }
                if (fav.Jogo.Empresa != null)
                {
                    afinidadeEstudios[fav.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(fav.Jogo.Empresa.Id) + 12.0;
                    nomesEstudiosFavoritos[fav.Jogo.Empresa.Id] = fav.Jogo.Empresa.NomeEmpresa;
                }
            }

            var avaliacoesCompletas = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            foreach (var av in avaliacoesCompletas.Where(a => a.Jogo != null))
            {
                double multiplicador = av.Nota >= 4 ? 2.5 : (av.Nota == 3 ? 1.0 : -1.5);
                double scoreAcao = av.Nota * multiplicador;

                if (av.Jogo.Generos != null)
                {
                    foreach (var g in av.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + scoreAcao;
                    }
                }
                if (av.Jogo.Empresa != null && av.Nota >= 4)
                {
                    afinidadeEstudios[av.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(av.Jogo.Empresa.Id) + scoreAcao;
                    nomesEstudiosFavoritos[av.Jogo.Empresa.Id] = av.Jogo.Empresa.NomeEmpresa;
                }
            }

            var bibliotecaCompleta = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == usuarioId && b.EstaAtivo)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            foreach (var bib in bibliotecaCompleta.Where(b => b.Jogo != null))
            {
                if (bib.Jogo.Generos != null)
                {
                    foreach (var g in bib.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + 4.0;
                    }
                }
                if (bib.Jogo.Empresa != null)
                {
                    afinidadeEstudios[bib.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(bib.Jogo.Empresa.Id) + 3.0;
                    nomesEstudiosFavoritos[bib.Jogo.Empresa.Id] = bib.Jogo.Empresa.NomeEmpresa;
                }
            }

            var seguidosIds = await _context.SegueUsuarios
                .AsNoTracking()
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo)
                .Select(s => s.UsuarioSeguido.Id)
                .ToListAsync();

            var socialBoostJogos = new Dictionary<Guid, double>();
            if (seguidosIds.Any())
            {
                var avaliacoesSeguidos = await _context.Avaliacoes
                    .AsNoTracking()
                    .Where(a => seguidosIds.Contains(a.Usuario.Id) && a.EstaAtivo && a.Nota >= 4)
                    .Select(a => new { JogoId = a.Jogo.Id, Nota = a.Nota })
                    .ToListAsync();

                foreach (var av in avaliacoesSeguidos)
                {
                    socialBoostJogos[av.JogoId] = socialBoostJogos.GetValueOrDefault(av.JogoId) + (av.Nota * 1.5);
                }
            }

            var generosPositivos = afinidadeGeneros
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(6)
                .ToList();

            if (!generosPositivos.Any() && !afinidadeEstudios.Any())
            {
                var jogosBase = await _context.Jogos
                    .AsNoTracking()
                    .Include(j => j.Generos)
                    .Include(j => j.Empresa)
                    .Where(j => j.EstaAtivo && !jogosExcluidos.Contains(j.Id))
                    .ToListAsync();

                var statsGeral = await _context.Avaliacoes
                    .AsNoTracking()
                    .Where(a => a.EstaAtivo)
                    .GroupBy(a => a.Jogo.Id)
                    .Select(g => new
                    {
                        JogoId = g.Key,
                        Media = g.Average(x => (double)x.Nota),
                        Total = g.Count()
                    })
                    .ToDictionaryAsync(x => x.JogoId);

                var topObrasPrimas = jogosBase
                    .Select(j =>
                    {
                        statsGeral.TryGetValue(j.Id, out var s);
                        return new
                        {
                            Jogo = j,
                            Media = s != null ? (double?)s.Media : null,
                            TotalReviews = s?.Total ?? 0
                        };
                    })
                    .OrderByDescending(x => x.TotalReviews > 0 ? (x.Media ?? 0) : 0)
                    .ThenByDescending(x => x.TotalReviews)
                    .ThenByDescending(x => x.Jogo.DataLancamento)
                    .Take(12)
                    .ToList();

                var resultadoObrasPrimas = topObrasPrimas.Select(item => new JogoRecomendacaoDTO
                {
                    JogoId = item.Jogo.Id,
                    Titulo = item.Jogo.Titulo,
                    Descricao = item.Jogo.Descricao,
                    Imagem = item.Jogo.Imagem,
                    DataLancamento = item.Jogo.DataLancamento,
                    ClassificacaoIndicativa = item.Jogo.ClassificacaoIndicativa,
                    GeneroFavorito = item.Jogo.Generos.FirstOrDefault()?.TituloGenero ?? "Destaque",
                    NomeEmpresa = item.Jogo.Empresa?.NomeEmpresa,
                    MediaAvaliacoes = item.Media.HasValue ? Math.Round(item.Media.Value, 1) : null,
                    MotivoRecomendacao = item.TotalReviews > 0 ? "Aclamado pela Comunidade" : "Destaque do Catálogo",
                    Score = (item.Media ?? 4.0) * 10
                }).ToList();

                _cache.Set(cacheKey, resultadoObrasPrimas, TimeSpan.FromMinutes(10));
                return resultadoObrasPrimas;
            }

            var afinidadeEstudiosIds = afinidadeEstudios.Keys.ToList();

            var candidatos = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Where(j => j.EstaAtivo && !jogosExcluidos.Contains(j.Id) &&
                           (j.Generos.Any(g => generosPositivos.Contains(g.TituloGenero)) ||
                            (j.Empresa != null && afinidadeEstudiosIds.Contains(j.Empresa.Id))))
                .ToListAsync();

            var candidatosIds = candidatos.Select(c => c.Id).ToList();
            var statsCandidatos = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && candidatosIds.Contains(a.Jogo.Id))
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.JogoId, x => x.Media);

            var pontuados = new List<JogoRecomendacaoDTO>();

            foreach (var j in candidatos)
            {
                double scoreFinal = 0.0;
                string motivo = string.Empty;
                string generoDestaque = generosPositivos.FirstOrDefault() ?? "Recomendado";

                double scoreGeneroJogo = 0.0;
                int generosCombinados = 0;
                string? melhorGeneroMatch = null;

                foreach (var g in j.Generos)
                {
                    if (afinidadeGeneros.TryGetValue(g.TituloGenero, out double pesoG))
                    {
                        scoreGeneroJogo += pesoG;
                        generosCombinados++;
                        if (melhorGeneroMatch == null || pesoG > afinidadeGeneros.GetValueOrDefault(melhorGeneroMatch))
                        {
                            melhorGeneroMatch = g.TituloGenero;
                        }
                    }
                }

                if (generosCombinados > 1)
                {
                    scoreGeneroJogo *= 1.25;
                }

                scoreFinal += scoreGeneroJogo;
                if (melhorGeneroMatch != null)
                {
                    generoDestaque = melhorGeneroMatch;
                    motivo = $"Baseado no seu gosto por {melhorGeneroMatch}";
                }

                if (j.Empresa != null && afinidadeEstudios.TryGetValue(j.Empresa.Id, out double pesoEstudio))
                {
                    scoreFinal += pesoEstudio * 1.4;
                    motivo = $"Do estúdio {j.Empresa.NomeEmpresa}";
                }

                statsCandidatos.TryGetValue(j.Id, out double mediaComunidade);
                if (mediaComunidade > 0)
                {
                    scoreFinal += (mediaComunidade * 3.0);
                }

                if (socialBoostJogos.TryGetValue(j.Id, out double socialBonus))
                {
                    scoreFinal += socialBonus;
                    if (string.IsNullOrEmpty(motivo))
                    {
                        motivo = "Popular entre quem você segue";
                    }
                }

                int anosDesdeLancamento = Math.Max(0, DateTime.UtcNow.Year - j.DataLancamento.Year);
                if (anosDesdeLancamento <= 2)
                {
                    scoreFinal += 6.0;
                }
                else if (anosDesdeLancamento <= 5)
                {
                    scoreFinal += 3.0;
                }

                pontuados.Add(new JogoRecomendacaoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    GeneroFavorito = generoDestaque,
                    NomeEmpresa = j.Empresa?.NomeEmpresa,
                    MediaAvaliacoes = mediaComunidade > 0 ? Math.Round(mediaComunidade, 1) : null,
                    MotivoRecomendacao = string.IsNullOrEmpty(motivo) ? "Recomendado para o seu perfil" : motivo,
                    Score = scoreFinal
                });
            }

            var ordenadosPorScore = pontuados
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.MediaAvaliacoes ?? 0)
                .ToList();

            var selecionados = new List<JogoRecomendacaoDTO>();
            var contagemPorEstudio = new Dictionary<string, int>();

            foreach (var item in ordenadosPorScore)
            {
                var est = item.NomeEmpresa ?? "Outro";
                int qtd = contagemPorEstudio.GetValueOrDefault(est);
                if (qtd < 2)
                {
                    selecionados.Add(item);
                    contagemPorEstudio[est] = qtd + 1;
                    if (selecionados.Count >= 12) break;
                }
            }

            if (selecionados.Count < 12)
            {
                var restantes = ordenadosPorScore
                    .Where(p => !selecionados.Any(s => s.JogoId == p.JogoId))
                    .Take(12 - selecionados.Count);
                selecionados.AddRange(restantes);
            }

            _cache.Set(cacheKey, selecionados, TimeSpan.FromMinutes(10));
            return selecionados;
        }
    }
}
