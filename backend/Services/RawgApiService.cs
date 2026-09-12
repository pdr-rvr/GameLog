using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Helpers;
using GameLog_Backend.Services.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.Services
{
    public class RawgApiService
    {
        private readonly HttpClient _httpClient;
        private readonly GameLogContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RawgApiService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public RawgApiService(
            HttpClient httpClient,
            GameLogContext context,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<RawgApiService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _cache = cache;
            _logger = logger;

            _apiKey = Environment.GetEnvironmentVariable("RAWG_API_KEY") 
                ?? configuration["Rawg:ApiKey"] 
                ?? "c542e67aec3a4340908f9de9e86038af";

            _baseUrl = configuration["Rawg:BaseUrl"] ?? "https://api.rawg.io/api";
        }

        public static bool EhJogoValido(RawgGameItemDTO? item)
        {
            if (item == null) return false;
            if (!CatalogSanitizer.EhJogoValido(item.Name)) return false;

            // DLCs e expansões com parent associado na RAWG
            if (item.ParentsCount > 0) return false;

            // Tags inválidas de fangames, romhacks, demakes, dlcs e ports não oficiais
            if (item.Tags != null && item.Tags.Any())
            {
                var invalidTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "fangame", "fan-game", "demake", "dlc", "addon", "add-on",
                    "soundtrack", "expansion", "gameport", "romhack", "rpg-maker", "scratch"
                };

                if (item.Tags.Any(t => !string.IsNullOrWhiteSpace(t.Slug) && invalidTags.Contains(t.Slug)))
                {
                    return false;
                }
            }

            // Validação de relevância/legitimidade para descartar clones vazios e projetos de teste
            var added = item.Added ?? 0;
            var ratingsCount = item.RatingsCount ?? 0;
            var hasMetacritic = item.Metacritic.HasValue;

            if (added < 20 && ratingsCount < 3 && !hasMetacritic)
            {
                return false;
            }

            return true;
        }

        public static bool EhJogoValido(string? name) => CatalogSanitizer.EhJogoValido(name);

        public async Task<List<RawgGameItemDTO>> ObterJogosPopularesRawg(int pagina, int pageSize = 40)
        {
            return await ObterJogosRawgQueryAsync("ordering=-added", pagina, pageSize);
        }

        public async Task<List<RawgGameItemDTO>> ObterJogosRawgQueryAsync(string queryParams, int pagina, int pageSize = 40)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.5));
                var cleanQuery = queryParams.TrimStart('&', '?');
                var url = $"{_baseUrl}/games?key={_apiKey}&{cleanQuery}&page={pagina}&page_size={pageSize}";
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url, cts.Token);
                if (response?.Results != null)
                {
                    return response.Results
                        .Where(r => EhJogoValido(r) && !string.IsNullOrWhiteSpace(r.BackgroundImage) && r.BackgroundImage.StartsWith("http"))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao obter página {Pagina} da RAWG com query '{Query}'", pagina, queryParams);
            }
            return new List<RawgGameItemDTO>();
        }

        public async Task<RawgSearchResultDTO> BuscarJogosExternos(string termo, int pagina = 1, int itensPorPagina = 20)
        {
            if (string.IsNullOrWhiteSpace(termo) || termo.Trim().Length < 2)
            {
                return new RawgSearchResultDTO();
            }

            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 40);
            var cacheKey = $"rawg_search_{termo.Trim().ToLower()}_{pagina}_{itensPorPagina}";

            if (_cache.TryGetValue(cacheKey, out RawgSearchResultDTO? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.5));
                var url = $"{_baseUrl}/games?key={_apiKey}&search={Uri.EscapeDataString(termo.Trim())}&page={pagina}&page_size={itensPorPagina}&search_precise=true";
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url, cts.Token);

                if (response == null || response.Results == null)
                {
                    return new RawgSearchResultDTO();
                }

                // Filtrar DLCs, fangames e itens irrelevantes à busca
                var jogosValidos = response.Results
                    .Where(r => EhJogoValido(r) && RelevanciaBuscaHelper.CorrespondeBusca(r.Name, null, termo))
                    .ToList();

                // Carregar títulos locais para marcar os que já estão importados
                var titulosBusca = jogosValidos.Select(r => r.Name.Trim().ToLower()).ToList();
                var jogosLocais = await _context.Jogos
                    .AsNoTracking()
                    .Where(j => j.EstaAtivo && titulosBusca.Contains(j.Titulo.ToLower()))
                    .Select(j => new { j.Id, TituloLower = j.Titulo.ToLower() })
                    .ToListAsync();

                var dictLocais = jogosLocais
                    .GroupBy(j => j.TituloLower)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var itensDTO = jogosValidos.Select(r =>
                {
                    int? ano = null;
                    if (!string.IsNullOrWhiteSpace(r.Released) && DateTime.TryParse(r.Released, out var dt))
                    {
                        ano = dt.Year;
                    }

                    var tituloLower = r.Name.Trim().ToLower();
                    dictLocais.TryGetValue(tituloLower, out var localId);

                    return new RawgSearchResultItemDTO
                    {
                        RawgId = r.Id,
                        Titulo = r.Name,
                        Imagem = r.BackgroundImage ?? "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80",
                        AnoLancamento = ano,
                        DataLancamento = r.Released,
                        NomeEmpresa = r.Publishers.FirstOrDefault()?.Name ?? r.Developers.FirstOrDefault()?.Name,
                        Generos = r.Genres.Select(g => g.Name).ToList(),
                        NotaRawg = r.Rating,
                        JaImportado = localId != Guid.Empty,
                        LocalJogoId = localId != Guid.Empty ? localId : null
                    };
                }).ToList();

                var resultado = new RawgSearchResultDTO
                {
                    TotalResultados = response.Count,
                    PaginaAtual = pagina,
                    Jogos = itensDTO
                };

                _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(10));
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ou timeout ao consultar RAWG API para termo '{Termo}'. Fallback para banco local ativado.", termo);
                return new RawgSearchResultDTO();
            }
        }

        public static string MapearGeneroParaPortugues(string rawgGenre) => GenreTaxonomyService.MapearGenero(rawgGenre);

        public static int MapearEsrbParaClassificacao(string? esrbSlug)
        {
            if (string.IsNullOrWhiteSpace(esrbSlug)) return 0;
            var slug = esrbSlug.Trim().ToLowerInvariant();
            if (slug.Contains("mature") || slug.Contains("adult")) return 18;
            if (slug.Contains("teen")) return 14;
            if (slug.Contains("10") || slug.Contains("everyone-10-plus")) return 10;
            if (slug.Contains("everyone") || slug.Contains("early-childhood")) return 0;
            return 0;
        }

        public static string? ResolverEmpresaPorFranquia(string titulo) => CompanyNormalizer.ResolverEmpresaPorFranquia(titulo);

        public async Task<List<JogoDTO>> BuscarJogosExternosFormatados(
            string? busca,
            int? ano,
            string? genero,
            string? empresa,
            int maxItens = 40)
        {
            if (string.IsNullOrWhiteSpace(busca) || busca.Trim().Length < 2)
            {
                return new List<JogoDTO>();
            }

            var queryParts = new List<string>
            {
                $"search={Uri.EscapeDataString(busca.Trim())}&search_precise=true"
            };

            if (ano.HasValue)
            {
                queryParts.Add($"dates={ano.Value:D4}-01-01,{ano.Value:D4}-12-31");
            }

            var queryString = string.Join("&", queryParts);
            var cacheKey = $"rawg_hybrid_{queryString.ToLower()}_{genero?.ToLower()}_{empresa?.ToLower()}_{maxItens}";

            if (_cache.TryGetValue(cacheKey, out List<JogoDTO>? cached) && cached != null)
            {
                return cached;
            }

            var url = $"{_baseUrl}/games?key={_apiKey}&{queryString}&page=1&page_size={Math.Min(maxItens, 40)}";
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.5));
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url, cts.Token);
                if (response?.Results == null || !response.Results.Any())
                {
                    return new List<JogoDTO>();
                }

                var validos = response.Results
                    .Where(r => EhJogoValido(r) 
                             && !string.IsNullOrWhiteSpace(r.BackgroundImage) 
                             && r.BackgroundImage.StartsWith("http")
                             && RelevanciaBuscaHelper.CorrespondeBusca(r.Name, null, busca))
                    .ToList();

                // Filtrar por gênero se solicitado
                if (!string.IsNullOrWhiteSpace(genero))
                {
                    var genTerm = genero.Trim().ToLower();
                    validos = validos.Where(r => r.Genres != null && r.Genres.Any(g => MapearGeneroParaPortugues(g.Name).ToLower().Contains(genTerm))).ToList();
                }

                // Filtrar por ano se solicitado
                if (ano.HasValue)
                {
                    validos = validos.Where(r => {
                        if (!string.IsNullOrWhiteSpace(r.Released) && DateTime.TryParse(r.Released, out var dt))
                        {
                            return dt.Year == ano.Value;
                        }
                        return false;
                    }).ToList();
                }

                // Identificar títulos já presentes no banco de dados local
                var titulos = validos.Select(v => v.Name.Trim().ToLower()).ToList();
                var jogosLocais = await _context.Jogos
                    .AsNoTracking()
                    .Include(j => j.Empresa)
                    .Include(j => j.Publicadora)
                    .Include(j => j.Generos)
                    .Where(j => j.EstaAtivo && titulos.Contains(j.Titulo.ToLower()))
                    .ToListAsync();

                var titulosLocaisDict = jogosLocais
                    .GroupBy(j => j.Titulo.Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.First());

                var resultado = new List<JogoDTO>();

                // Para jogos novos da RAWG não presentes no banco local, enriquecer estúdio e detalhes
                var externosNovos = validos.Where(r => !titulosLocaisDict.ContainsKey(r.Name.Trim().ToLower())).Take(15).ToList();

                // Buscar detalhes em paralelo para obter publisher/developer real e filtrar DLCs
                var detailTasks = externosNovos.Select(async ext =>
                {
                    var detail = await ObterDetalhesJogoExterno(ext.Id);
                    return (Ext: ext, Detail: detail);
                });

                var detailsResolved = await Task.WhenAll(detailTasks);
                var detailsDict = new Dictionary<int, RawgGameDetailDTO>();
                foreach (var (ext, detail) in detailsResolved)
                {
                    if (detail != null)
                    {
                        detailsDict[ext.Id] = detail;
                    }
                }

                foreach (var r in validos)
                {
                    var titLower = r.Name.Trim().ToLower();
                    if (titulosLocaisDict.TryGetValue(titLower, out var localJogo))
                    {
                        resultado.Add(new JogoDTO
                        {
                            JogoId = localJogo.Id,
                            RawgId = r.Id,
                            Titulo = localJogo.Titulo,
                            Descricao = localJogo.Descricao,
                            Imagem = localJogo.Imagem,
                            DataLancamento = localJogo.DataLancamento,
                            ClassificacaoIndicativa = localJogo.ClassificacaoIndicativa,
                            EmpresaId = localJogo.Empresa?.Id ?? Guid.Empty,
                            NomeEmpresa = localJogo.Empresa?.NomeEmpresa ?? string.Empty,
                            PublicadoraId = localJogo.Publicadora?.Id,
                            NomePublicadora = localJogo.Publicadora?.NomeEmpresa,
                            EstaAtivo = true,
                            Generos = localJogo.Generos?.Select(g => g.TituloGenero).ToList() ?? new List<string>(),
                            EhExterno = false
                        });
                    }
                    else
                    {
                        detailsDict.TryGetValue(r.Id, out var detail);

                        // Se for uma DLC / expansão (parents_count > 0), descartar dos resultados
                        if (detail != null && detail.ParentsCount > 0)
                        {
                            continue;
                        }

                        DateOnly dataLanc = new DateOnly(2020, 1, 1);
                        var releasedStr = detail?.Released ?? r.Released;
                        if (!string.IsNullOrWhiteSpace(releasedStr) && DateOnly.TryParse(releasedStr, out var parsedData))
                        {
                            dataLanc = parsedData;
                        }

                        var devRaw = detail?.Developers?.FirstOrDefault()?.Name ?? r.Developers?.FirstOrDefault()?.Name;
                        var pubRaw = detail?.Publishers?.FirstOrDefault()?.Name ?? r.Publishers?.FirstOrDefault()?.Name ?? ResolverEmpresaPorFranquia(r.Name);

                        var nomeDev = CompanyNormalizer.NormalizarNomeEmpresa(devRaw ?? pubRaw);
                        var nomePub = !string.IsNullOrWhiteSpace(pubRaw) ? CompanyNormalizer.NormalizarNomeEmpresa(pubRaw) : null;

                        var generosSource = (detail?.Genres != null && detail.Genres.Any()) ? detail.Genres : r.Genres;
                        var generosNormalizados = generosSource?
                            .Select(g => MapearGeneroParaPortugues(g.Name))
                            .Distinct()
                            .ToList() ?? new List<string>();

                        if (!generosNormalizados.Any())
                        {
                            generosNormalizados.Add("Ação");
                        }

                        int classificacao = MapearEsrbParaClassificacao(detail?.EsrbRating?.Slug ?? r.EsrbRating?.Slug);

                        resultado.Add(new JogoDTO
                        {
                            JogoId = Guid.Empty,
                            RawgId = r.Id,
                            Titulo = r.Name.Trim(),
                            Descricao = detail?.DescriptionRaw ?? string.Empty,
                            Imagem = detail?.BackgroundImage ?? r.BackgroundImage ?? "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80",
                            DataLancamento = dataLanc,
                            ClassificacaoIndicativa = classificacao,
                            EmpresaId = Guid.Empty,
                            NomeEmpresa = nomeDev,
                            PublicadoraId = null,
                            NomePublicadora = (nomePub != null && !string.Equals(nomePub, nomeDev, StringComparison.OrdinalIgnoreCase)) ? nomePub : null,
                            EstaAtivo = true,
                            Generos = generosNormalizados,
                            MediaAvaliacoes = null,
                            TotalAvaliacoes = 0,
                            EhExterno = true
                        });
                    }
                }

                _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(10));
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ou timeout ao buscar jogos híbridos na RAWG para query '{Query}'. Utilizando catálogo local.", queryString);
                return new List<JogoDTO>();
            }
        }

        public async Task<RawgGameDetailDTO?> ObterDetalhesJogoExterno(int rawgId)
        {
            var cacheKey = $"rawg_detail_{rawgId}";
            if (_cache.TryGetValue(cacheKey, out RawgGameDetailDTO? cachedDetail) && cachedDetail != null)
            {
                return cachedDetail;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3.5));
                var url = $"{_baseUrl}/games/{rawgId}?key={_apiKey}";
                var detail = await _httpClient.GetFromJsonAsync<RawgGameDetailDTO>(url, cts.Token);

                if (detail != null)
                {
                    _cache.Set(cacheKey, detail, TimeSpan.FromHours(1));
                }

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ou timeout ao obter detalhes da RAWG para jogo ID {RawgId}", rawgId);
                return null;
            }
        }

        public async Task<JogoDTO?> ImportarJogoRawgParaBanco(int rawgId)
        {
            var detail = await ObterDetalhesJogoExterno(rawgId);
            if (detail == null || string.IsNullOrWhiteSpace(detail.Name) || !EhJogoValido(detail.Name))
            {
                return null;
            }

            var tituloLimpo = detail.Name.Trim();
            if (tituloLimpo.Length > 250)
            {
                tituloLimpo = tituloLimpo.Substring(0, 250).Trim();
            }

            // Verificar se já existe no banco local
            var jogoExistente = await _context.Jogos
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .Include(j => j.Generos)
                .FirstOrDefaultAsync(j => j.EstaAtivo && j.Titulo.ToLower() == tituloLimpo.ToLower());

            if (jogoExistente != null)
            {
                return MapearParaDTO(jogoExistente);
            }

            // 1. Obter ou Criar Desenvolvedora e Publicadora Canônicas
            var devRaw = detail.Developers.FirstOrDefault()?.Name ?? detail.Publishers.FirstOrDefault()?.Name;
            var pubRaw = detail.Publishers.FirstOrDefault()?.Name ?? ResolverEmpresaPorFranquia(detail.Name);

            var nomeDev = CompanyNormalizer.NormalizarNomeEmpresa(devRaw ?? pubRaw);
            var nomePub = !string.IsNullOrWhiteSpace(pubRaw) ? CompanyNormalizer.NormalizarNomeEmpresa(pubRaw) : null;

            if (string.IsNullOrWhiteSpace(nomeDev))
            {
                _logger.LogWarning("Jogo '{Titulo}' ignorado pois não possui empresa/estúdio válido na RAWG", tituloLimpo);
                return null;
            }

            if (nomeDev.Length > 150) nomeDev = nomeDev.Substring(0, 150).Trim();

            var devEmpresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.NomeEmpresa.ToLower() == nomeDev.ToLower());

            if (devEmpresa == null)
            {
                devEmpresa = new Empresa
                {
                    NomeEmpresa = nomeDev,
                    EstaAtivo = true
                };
                _context.Empresa.Add(devEmpresa);
                await _context.SaveChangesAsync();
            }

            Empresa? pubEmpresa = null;
            if (!string.IsNullOrWhiteSpace(nomePub) && !string.Equals(nomePub, nomeDev, StringComparison.OrdinalIgnoreCase))
            {
                if (nomePub.Length > 150) nomePub = nomePub.Substring(0, 150).Trim();
                pubEmpresa = await _context.Empresa
                    .FirstOrDefaultAsync(e => e.NomeEmpresa.ToLower() == nomePub.ToLower());

                if (pubEmpresa == null)
                {
                    pubEmpresa = new Empresa
                    {
                        NomeEmpresa = nomePub,
                        EstaAtivo = true
                    };
                    _context.Empresa.Add(pubEmpresa);
                    await _context.SaveChangesAsync();
                }
            }

            // 2. Obter ou Criar Gêneros Canônicos em Português
            var generosEntities = new List<Genero>();
            var generosNomes = detail.Genres
                .Select(g => MapearGeneroParaPortugues(g.Name))
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .ToList();

            if (!generosNomes.Any())
            {
                generosNomes.Add("Ação");
            }

            foreach (var genNome in generosNomes)
            {
                var nomeTratado = genNome;
                if (nomeTratado.Length > 50) nomeTratado = nomeTratado.Substring(0, 50).Trim();

                var gen = await _context.Generos
                    .FirstOrDefaultAsync(g => g.TituloGenero.ToLower() == nomeTratado.ToLower());

                if (gen == null)
                {
                    gen = new Genero
                    {
                        TituloGenero = nomeTratado,
                        EstaAtivo = true
                    };
                    _context.Generos.Add(gen);
                    await _context.SaveChangesAsync();
                }

                generosEntities.Add(gen);
            }

            // 3. Processar Data de Lançamento
            var dataLancamento = new DateOnly(2020, 1, 1);
            if (!string.IsNullOrWhiteSpace(detail.Released) && DateOnly.TryParse(detail.Released, out var dLanc))
            {
                dataLancamento = dLanc;
            }

            // 4. Processar Classificação Indicativa
            int classificacao = MapearEsrbParaClassificacao(detail.EsrbRating?.Slug ?? detail.EsrbRating?.Name);

            // 5. Descrição limpa
            var descricao = detail.DescriptionRaw;
            if (string.IsNullOrWhiteSpace(descricao) && !string.IsNullOrWhiteSpace(detail.DescriptionHtml))
            {
                descricao = Regex.Replace(detail.DescriptionHtml, "<.*?>", string.Empty).Trim();
            }
            if (string.IsNullOrWhiteSpace(descricao))
            {
                descricao = $"{tituloLimpo} é um aclamado jogo lançado em {dataLancamento.Year} pela {nomeDev}.";
            }

            // 6. Criar Jogo com Capa Oficial
            var imagemOficial = detail.BackgroundImage;
            if (string.IsNullOrWhiteSpace(imagemOficial))
            {
                imagemOficial = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80";
            }

            var novoJogo = new Jogo
            {
                Titulo = tituloLimpo,
                Descricao = descricao,
                Imagem = imagemOficial,
                DataLancamento = dataLancamento,
                ClassificacaoIndicativa = classificacao,
                Empresa = devEmpresa,
                Publicadora = pubEmpresa,
                Generos = generosEntities,
                EstaAtivo = true
            };

            _context.Jogos.Add(novoJogo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Jogo real '{Titulo}' importado com sucesso da RAWG com ID local {LocalId}", novoJogo.Titulo, novoJogo.Id);

            return MapearParaDTO(novoJogo);
        }

        private static JogoDTO MapearParaDTO(Jogo j)
        {
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
                Generos = j.Generos?.Select(g => g.TituloGenero).ToList() ?? new List<string>(),
                MediaAvaliacoes = null,
                TotalAvaliacoes = 0
            };
        }
    }
}
