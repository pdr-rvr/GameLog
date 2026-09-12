using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class JogoServices
    {
        protected readonly GameLogContext _context;
        protected readonly RawgApiService _rawgApiService;

        public JogoServices(GameLogContext context, RawgApiService rawgApiService)
        {
            _context = context;
            _rawgApiService = rawgApiService;
        }

        public async Task<IEnumerable<JogoDTO>> ListarJogos()
        {
            var jogos = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .ToListAsync();

            if (!jogos.Any())
                return Enumerable.Empty<JogoDTO>();

            var stats = await _context.Avaliacoes
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

            return jogos.Select(j =>
            {
                stats.TryGetValue(j.Id, out var s);
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
                    MediaAvaliacoes = s != null ? s.Media : null,
                    TotalAvaliacoes = s?.Total ?? 0,
                    EhExterno = false
                };
            }).ToList();
        }

        public async Task<PagedResult<JogoDTO>> ListarJogosPaginados(
            int pagina = 1,
            int itensPorPagina = 12,
            string? busca = null,
            IEnumerable<string>? generos = null,
            int? ano = null,
            string? empresa = null,
            double? notaMinima = null,
            string ordenacao = "melhores")
        {
            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

            var query = _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .Where(j => j.EstaAtivo);

            // Filtro textual por termo de busca
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(j => j.Titulo.ToLower().Contains(termo)
                                      || (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo))
                                      || (j.Publicadora != null && j.Publicadora.NomeEmpresa.ToLower().Contains(termo))
                                      || j.Generos.Any(g => g.TituloGenero.ToLower().Contains(termo)));
            }

            // Filtro composto por múltiplos gêneros/subgêneros (AND logic)
            if (generos != null)
            {
                var listaGeneros = generos
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g.Trim().ToLower())
                    .Distinct()
                    .ToList();

                foreach (var gTerm in listaGeneros)
                {
                    var termoG = gTerm;
                    query = query.Where(j => j.Generos.Any(g => g.TituloGenero.ToLower() == termoG || g.TituloGenero.ToLower().Contains(termoG)));
                }
            }

            if (ano.HasValue)
            {
                query = query.Where(j => j.DataLancamento.Year == ano.Value);
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var empresaTerm = empresa.Trim().ToLower();
                query = query.Where(j => (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(empresaTerm))
                                      || (j.Publicadora != null && j.Publicadora.NomeEmpresa.ToLower().Contains(empresaTerm)));
            }

            // Projeção dos jogos com médias de avaliação diretamente do banco local
            var jogosQuery = query.Select(j => new JogoDTO
            {
                JogoId = j.Id,
                Titulo = j.Titulo,
                Descricao = j.Descricao,
                Imagem = j.Imagem,
                DataLancamento = j.DataLancamento,
                ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                EmpresaId = j.Empresa != null ? j.Empresa.Id : Guid.Empty,
                NomeEmpresa = j.Empresa != null ? j.Empresa.NomeEmpresa : string.Empty,
                PublicadoraId = j.Publicadora != null ? j.Publicadora.Id : null,
                NomePublicadora = j.Publicadora != null ? j.Publicadora.NomeEmpresa : null,
                EstaAtivo = j.EstaAtivo,
                Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                MediaAvaliacoes = _context.Avaliacoes
                    .Where(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                    .Average(a => (double?)a.Nota),
                TotalAvaliacoes = _context.Avaliacoes
                    .Count(a => a.Jogo.Id == j.Id && a.EstaAtivo),
                EhExterno = false
            });

            if (notaMinima.HasValue)
            {
                jogosQuery = jogosQuery.Where(j => j.MediaAvaliacoes != null && j.MediaAvaliacoes >= notaMinima.Value);
            }

            var totalItens = await jogosQuery.CountAsync();

            jogosQuery = (ordenacao ?? "melhores").ToLower() switch
            {
                "recentes" => jogosQuery.OrderByDescending(j => j.DataLancamento).ThenByDescending(j => j.MediaAvaliacoes ?? 0),
                "antigos" => jogosQuery.OrderBy(j => j.DataLancamento).ThenByDescending(j => j.MediaAvaliacoes ?? 0),
                "az" => jogosQuery.OrderBy(j => j.Titulo),
                "za" => jogosQuery.OrderByDescending(j => j.Titulo),
                "populares" => jogosQuery.OrderByDescending(j => j.TotalAvaliacoes).ThenByDescending(j => j.MediaAvaliacoes ?? 0),
                _ => jogosQuery.OrderByDescending(j => j.MediaAvaliacoes ?? 0).ThenByDescending(j => j.TotalAvaliacoes).ThenByDescending(j => j.DataLancamento)
            };

            var itensPaginados = await jogosQuery
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            return new PagedResult<JogoDTO>(itensPaginados, totalItens, pagina, itensPorPagina);
        }

        public async Task<JogoDTO?> ObterJogoPorId(Guid id)
        {
            var jogo = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.Id == id && j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .FirstOrDefaultAsync();

            if (jogo == null) return null;

            var stats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Jogo.Id == id && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    Media = g.Average(x => (double)x.Nota),
                    Total = g.Count()
                })
                .FirstOrDefaultAsync();

            return new JogoDTO
            {
                JogoId = jogo.Id,
                Titulo = jogo.Titulo,
                Descricao = jogo.Descricao,
                Imagem = jogo.Imagem,
                DataLancamento = jogo.DataLancamento,
                ClassificacaoIndicativa = jogo.ClassificacaoIndicativa,
                EmpresaId = jogo.Empresa?.Id ?? Guid.Empty,
                NomeEmpresa = jogo.Empresa?.NomeEmpresa ?? string.Empty,
                PublicadoraId = jogo.Publicadora?.Id,
                NomePublicadora = jogo.Publicadora?.NomeEmpresa,
                EstaAtivo = jogo.EstaAtivo,
                Generos = jogo.Generos.Select(g => g.TituloGenero).ToList(),
                MediaAvaliacoes = stats?.Media,
                TotalAvaliacoes = stats?.Total ?? 0
            };
        }

        public async Task<IEnumerable<JogoDTO>> ListarTop10JogosMelhorAvaliados()
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo.EstaAtivo)
                .GroupBy(a => a.Jogo)
                .Select(g => new JogoDTO
                {
                    JogoId = g.Key.Id,
                    Titulo = g.Key.Titulo,
                    Descricao = g.Key.Descricao,
                    Imagem = g.Key.Imagem,
                    DataLancamento = g.Key.DataLancamento,
                    ClassificacaoIndicativa = g.Key.ClassificacaoIndicativa,
                    EmpresaId = g.Key.Empresa.Id,
                    NomeEmpresa = g.Key.Empresa.NomeEmpresa,
                    PublicadoraId = g.Key.Publicadora != null ? g.Key.Publicadora.Id : null,
                    NomePublicadora = g.Key.Publicadora != null ? g.Key.Publicadora.NomeEmpresa : null,
                    EstaAtivo = g.Key.EstaAtivo,
                    MediaAvaliacoes = g.Average(a => (double?)a.Nota),
                    TotalAvaliacoes = g.Count(),
                    Generos = g.Key.Generos.Select(ge => ge.TituloGenero).ToList()
                })
                .OrderByDescending(j => j.MediaAvaliacoes)
                .ThenByDescending(j => j.DataLancamento)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<JogoDTO>> ListarDestaquesHeroAsync(int limite = 5)
        {
            limite = Math.Clamp(limite, 1, 10);
            var titulosCanonicicos = new[]
            {
                "The Witcher 3: Wild Hunt",
                "Red Dead Redemption 2",
                "Baldur's Gate III",
                "Cyberpunk 2077",
                "Elden Ring",
                "God of War",
                "The Legend of Zelda: Tears of the Kingdom",
                "Grand Theft Auto V"
            };

            var destaques = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .Where(j => j.EstaAtivo && !string.IsNullOrEmpty(j.Imagem) && titulosCanonicicos.Contains(j.Titulo))
                .Take(limite)
                .ToListAsync();

            if (destaques.Count < limite)
            {
                var destaquesIds = destaques.Select(d => d.Id).ToHashSet();
                var fallback = await _context.Jogos
                    .AsNoTracking()
                    .Include(j => j.Generos)
                    .Include(j => j.Empresa)
                    .Include(j => j.Publicadora)
                    .Where(j => j.EstaAtivo && !string.IsNullOrEmpty(j.Imagem) && !destaquesIds.Contains(j.Id))
                    .OrderByDescending(j => j.DataLancamento)
                    .Take(limite - destaques.Count)
                    .ToListAsync();
                destaques.AddRange(fallback);
            }

            var ids = destaques.Select(d => d.Id).ToList();
            var stats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => ids.Contains(a.Jogo.Id) && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    Media = g.Average(x => (double)x.Nota),
                    Total = g.Count()
                })
                .ToDictionaryAsync(x => x.JogoId);

            return destaques.Select(j =>
            {
                stats.TryGetValue(j.Id, out var s);
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
                    MediaAvaliacoes = s != null ? Math.Round(s.Media, 1) : 5.0, // Destaques padrão
                    TotalAvaliacoes = s?.Total ?? 0,
                    EhExterno = false
                };
            }).ToList();
        }

        public async Task<MetadadosFiltrosDTO> ObterMetadadosFiltros()
        {
            var generos = await _context.Generos
                .AsNoTracking()
                .Where(g => g.EstaAtivo && g.Jogos.Any(j => j.EstaAtivo))
                .OrderBy(g => g.TituloGenero)
                .Select(g => g.TituloGenero)
                .ToListAsync();

            var empresas = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo && j.Empresa != null && j.Empresa.EstaAtivo)
                .Select(j => j.Empresa.NomeEmpresa)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            var anoAtual = DateTime.UtcNow.Year + 1;
            var anos = Enumerable.Range(1985, anoAtual - 1985 + 1).OrderByDescending(a => a).ToList();

            return new MetadadosFiltrosDTO
            {
                Generos = generos,
                Empresas = empresas,
                Anos = anos
            };
        }
    }
}
