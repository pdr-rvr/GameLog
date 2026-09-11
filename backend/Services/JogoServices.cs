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
            string? genero = null,
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

            // Filtros com sanitização
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(j => j.Titulo.ToLower().Contains(termo)
                                      || (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo))
                                      || (j.Publicadora != null && j.Publicadora.NomeEmpresa.ToLower().Contains(termo)));
            }

            if (!string.IsNullOrWhiteSpace(genero))
            {
                var generoTerm = genero.Trim().ToLower();
                query = query.Where(j => j.Generos.Any(g => g.TituloGenero.ToLower() == generoTerm));
            }

            if (ano.HasValue)
            {
                query = query.Where(j => j.DataLancamento.Year == ano.Value);
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var empresaTerm = empresa.Trim().ToLower();
                query = query.Where(j => (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower() == empresaTerm)
                                      || (j.Publicadora != null && j.Publicadora.NomeEmpresa.ToLower() == empresaTerm));
            }

            // A consulta externa à RAWG é acionada EXCLUSIVAMENTE para complementar a barra de pesquisa textual ('busca')
            var temBuscaTextual = !string.IsNullOrWhiteSpace(busca) && busca.Trim().Length >= 2;

            if (temBuscaTextual)
            {
                var jogosLocais = await query.ToListAsync();
                var locaisIds = jogosLocais.Select(j => j.Id).ToList();

                var stats = await _context.Avaliacoes
                    .AsNoTracking()
                    .Where(a => a.EstaAtivo && locaisIds.Contains(a.Jogo.Id))
                    .GroupBy(a => a.Jogo.Id)
                    .Select(g => new
                    {
                        JogoId = g.Key,
                        Media = g.Average(x => (double)x.Nota),
                        Total = g.Count()
                    })
                    .ToDictionaryAsync(x => x.JogoId);

                var listaLocaisDTO = jogosLocais.Select(j =>
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

                List<JogoDTO> externos = new();
                try
                {
                    // Busca externa com fallback automático embutido
                    externos = await _rawgApiService.BuscarJogosExternosFormatados(busca, ano, genero, empresa, 60);
                }
                catch
                {
                    // Fallback transparente para o banco local
                    externos = new List<JogoDTO>();
                }

                var titulosLocais = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var listaFinal = new List<JogoDTO>();

                foreach (var local in listaLocaisDTO)
                {
                    if (RelevanciaBuscaHelper.CorrespondeBusca(local.Titulo, local.NomeEmpresa, local.NomePublicadora, busca))
                    {
                        if (notaMinima.HasValue && (local.MediaAvaliacoes == null || local.MediaAvaliacoes < notaMinima.Value))
                        {
                            continue;
                        }

                        titulosLocais.Add(local.Titulo.Trim().ToLower());
                        listaFinal.Add(local);
                    }
                }

                foreach (var ext in externos)
                {
                    var tLower = ext.Titulo.Trim().ToLower();
                    if (!titulosLocais.Contains(tLower) && RelevanciaBuscaHelper.CorrespondeBusca(ext.Titulo, ext.NomeEmpresa, ext.NomePublicadora, busca))
                    {
                        if (notaMinima.HasValue && (ext.MediaAvaliacoes == null || ext.MediaAvaliacoes < notaMinima.Value))
                        {
                            continue;
                        }

                        titulosLocais.Add(tLower);
                        listaFinal.Add(ext);
                    }
                }

                // Ordenação em memória com prioridade de relevância textual
                IEnumerable<JogoDTO> ordenados = (ordenacao ?? "melhores").ToLower() switch
                {
                    "recentes" => listaFinal.OrderByDescending(j => j.DataLancamento).ThenByDescending(j => j.MediaAvaliacoes ?? 0),
                    "antigos" => listaFinal.OrderBy(j => j.DataLancamento).ThenByDescending(j => j.MediaAvaliacoes ?? 0),
                    "az" => listaFinal.OrderBy(j => j.Titulo),
                    "za" => listaFinal.OrderByDescending(j => j.Titulo),
                    _ => listaFinal
                        .OrderByDescending(j => RelevanciaBuscaHelper.CalcularScoreRelevancia(j.Titulo, busca))
                        .ThenByDescending(j => j.MediaAvaliacoes ?? 0)
                        .ThenByDescending(j => j.DataLancamento)
                };

                var total = listaFinal.Count;
                var itens = ordenados.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina).ToList();

                return new PagedResult<JogoDTO>(itens, total, pagina, itensPorPagina);
            }

            // Caso padrão (Navegação geral do catálogo no banco local)
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
                "recentes" => jogosQuery.OrderByDescending(j => j.DataLancamento),
                "antigos" => jogosQuery.OrderBy(j => j.DataLancamento),
                "az" => jogosQuery.OrderBy(j => j.Titulo),
                "za" => jogosQuery.OrderByDescending(j => j.Titulo),
                _ => jogosQuery.OrderByDescending(j => j.MediaAvaliacoes ?? 0).ThenByDescending(j => j.DataLancamento)
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
