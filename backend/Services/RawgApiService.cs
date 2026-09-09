using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
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

        public static bool EhJogoValido(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var lower = name.ToLowerInvariant().Trim();

            if (Regex.IsMatch(lower, @"#\w+")) return false;

            // 1. Palavras-chave de DLCs, expansões, demos, pacotes, trilhas sonoras, betas, protótipos e demakes
            if (Regex.IsMatch(lower, @"\b(dlc|dlcs|expansion|expansions|soundtrack|soundtracks|score|season pass|expansion pass|battle pass|access pass|booster pack|booster|bonus content|deluxe upgrade|wallpaper engine|wallpaper|3dmark|soundpad|software|pre-order|free trial|playtest|closed beta|open beta|\bbeta\b|\balpha\b|prologue|demo|teaser|trailer|benchmark|test build|fanmade|fan game|fan-made|fangame|tribute|demake|game jam|gamejam|game clone|mod pack|blockout|toolkit|redkit|how to control my votes)\b") ||
                Regex.IsMatch(lower, @"(^|\s|\()ost(\s|\)|$)") ||
                Regex.IsMatch(lower, @"(^|\s|\()demo(\s|\)|$)") ||
                Regex.IsMatch(lower, @"\b(remix\s*\(|fanmade\s+boss)\b"))
            {
                return false;
            }

            // 2. Pacotes, Bundles e Multi-Packs
            if (Regex.IsMatch(lower, @"\b(pack|packs|bundle|bundles|two-pack|2-pack|double pack|triple pack|mission pack|upgrade pack|starter pack|character pack|skin pack|voice pack|costume pack|texture pack|item pack|clothing pack|weapon pack|bonus pack|item set|map pack)\b"))
            {
                return false;
            }

            // 3. Versões regionais duplicadas
            if (Regex.IsMatch(lower, @"\b(german edition|russian edition|french edition|spanish edition|japanese edition|chinese edition|italian edition|english edition|us edition|uk edition|pal version|ntsc version)\b"))
            {
                return false;
            }

            // 4. Episódios / Capítulos avulsos (preservando jogos canônicos como Half-Life 2: Episode One/Two e Star Wars Episode)
            if (Regex.IsMatch(lower, @"\b(episode\s+[0-9]+|ep\.\s*[0-9]+|chapter\s+[0-9]+)\b") &&
                !lower.Contains("half-life 2: episode") &&
                !lower.Contains("star wars episode") &&
                !lower.Contains("sonic the hedgehog 4"))
            {
                return false;
            }

            // 5. Expansões e DLCs famosas com subtítulos específicos
            var dlcSubtitles = new[]
            {
                "blood and wine", "hearts of stone", "the ringed city", "ashes of ariandel",
                "shadows of rose", "phantom liberty", "left behind", "shadow of the erdtree",
                "iceborne", "sunbreak", "separate ways", "the frozen wilds", "burning shores",
                "iki island", "burial at sea", "minerva's den", "dragonborn", "dawnguard",
                "hearthfire", "nuka-world", "far harbor", "automatron", "vault-tec workshop",
                "lonesome road", "old world blues", "honest hearts", "dead money", "point lookout",
                "broken steel", "the pitt", "mothership zeta", "operation: anchorage", "curse of the pharaohs",
                "the hidden ones", "legacy of the first blade", "the fate of atlantis", "dawn of ragnar",
                "undead nightmare", "harley quinn's revenge", "cold, cold heart", "a matter of family",
                "the delicious last course", "whistleblower", "the signal", "the writer", "night springs",
                "the lake house", "ancient gods", "end of zoe", "not a hero", "the consequence",
                "the assignment", "the executioner", "freedom cry", "knife of dunwall", "the brigmore witches",
                "the missing link", "commander lilith", "sinclair solutions", "the lost artifact",
                "gathering storm", "rise and fall", "the grim and the grave", "call of the beastmen",
                "banned footage", "watchpoint pack", "gage historical", "reverse cosplay", "element of destruction",
                "zinyak attack", "space pack", "yokohama massage", "expedition", "lost between worlds",
                "ghosts - invasion", "ghosts - onslaught", "ghosts - devastation", "ghosts - nemesis",
                "first strike", "rezurrection", "all-in-one package"
            };

            foreach (var sub in dlcSubtitles)
            {
                if (lower.Contains(sub, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // 6. Protótipos, mods e fangames não-oficiais (preservando jogos oficiais como Garry's Mod, Star Wars Clone Wars e Prototype)
            if (lower.Contains("itch.io") ||
                lower.Contains("(itch)") ||
                lower.Contains("notavirus") ||
                lower.Contains(".exe") ||
                lower.Contains("rejiggied") ||
                lower.Contains("buff sonic") ||
                lower.Contains("songs of the past") ||
                lower.Contains("witcher switcher") ||
                lower.Contains("the new path") ||
                lower.Contains("a contract") ||
                lower.Contains("witcher go") ||
                lower.Contains("scratch edition") ||
                lower.Contains("street fighter: scratch") ||
                lower.Contains("cost of hope") ||
                lower.Contains("lost prototype") ||
                lower.Contains("vector's lost world") ||
                lower.Contains("diablo but") ||
                Regex.IsMatch(lower, @"\b(mod tools|multiplayer mod|biohazard mod|cs:go mod|queue simulator|texturing)\b") ||
                (Regex.IsMatch(lower, @"\b(clone|clones)\b") && !lower.Contains("clone wars")) ||
                (Regex.IsMatch(lower, @"\b(mod|mods)\b") && !lower.Contains("garry's mod")) ||
                Regex.IsMatch(lower, @"prototype\s+(ver|version|no\.|-\d+|\d+\.\d+|rectangulo|movement)") ||
                Regex.IsMatch(lower, @"\b(movement prototype|demake prototype|prototype 2-week)\b"))
            {
                return false;
            }

            return true;
        }

        public async Task<List<RawgGameItemDTO>> ObterJogosPopularesRawg(int pagina, int pageSize = 40)
        {
            return await ObterJogosRawgQueryAsync("ordering=-added", pagina, pageSize);
        }

        public async Task<List<RawgGameItemDTO>> ObterJogosRawgQueryAsync(string queryParams, int pagina, int pageSize = 40)
        {
            try
            {
                var cleanQuery = queryParams.TrimStart('&', '?');
                var url = $"{_baseUrl}/games?key={_apiKey}&{cleanQuery}&page={pagina}&page_size={pageSize}";
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url);
                if (response?.Results != null)
                {
                    return response.Results
                        .Where(r => EhJogoValido(r.Name) && !string.IsNullOrWhiteSpace(r.BackgroundImage) && r.BackgroundImage.StartsWith("http"))
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
                var url = $"{_baseUrl}/games?key={_apiKey}&search={Uri.EscapeDataString(termo.Trim())}&page={pagina}&page_size={itensPorPagina}&search_precise=true";
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url);

                if (response == null || response.Results == null)
                {
                    return new RawgSearchResultDTO();
                }

                // Filtrar DLCs e itens sem imagem
                var jogosValidos = response.Results.Where(r => EhJogoValido(r.Name)).ToList();

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
                        JaImportado = localId > 0,
                        LocalJogoId = localId > 0 ? localId : null
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
                _logger.LogError(ex, "Erro ao consultar RAWG API para termo '{Termo}'", termo);
                return new RawgSearchResultDTO();
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
                var url = $"{_baseUrl}/games/{rawgId}?key={_apiKey}";
                var detail = await _httpClient.GetFromJsonAsync<RawgGameDetailDTO>(url);

                if (detail != null)
                {
                    _cache.Set(cacheKey, detail, TimeSpan.FromHours(1));
                }

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes da RAWG para jogo ID {RawgId}", rawgId);
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
                .Include(j => j.Generos)
                .FirstOrDefaultAsync(j => j.EstaAtivo && j.Titulo.ToLower() == tituloLimpo.ToLower());

            if (jogoExistente != null)
            {
                return MapearParaDTO(jogoExistente);
            }

            // 1. Obter ou Criar Empresa Oficial
            var nomeEmpresa = detail.Publishers.FirstOrDefault()?.Name 
                ?? detail.Developers.FirstOrDefault()?.Name;

            if (string.IsNullOrWhiteSpace(nomeEmpresa))
            {
                _logger.LogWarning("Jogo '{Titulo}' ignorado pois não possui empresa/estúdio válido na RAWG", tituloLimpo);
                return null;
            }

            nomeEmpresa = nomeEmpresa.Trim();
            if (nomeEmpresa.Length > 150)
            {
                nomeEmpresa = nomeEmpresa.Substring(0, 150).Trim();
            }

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.NomeEmpresa.ToLower() == nomeEmpresa.ToLower());

            if (empresa == null)
            {
                empresa = new Empresa
                {
                    NomeEmpresa = nomeEmpresa,
                    EstaAtivo = true
                };
                _context.Empresa.Add(empresa);
                await _context.SaveChangesAsync();
            }

            // 2. Obter ou Criar Gêneros
            var generosEntities = new List<Genero>();
            var generosNomes = detail.Genres.Select(g => g.Name.Trim()).Where(g => !string.IsNullOrEmpty(g)).ToList();

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
            int classificacao = 0;
            var esrb = detail.EsrbRating?.Name?.ToLower() ?? "";
            if (esrb.Contains("mature") || esrb.Contains("adults")) classificacao = 18;
            else if (esrb.Contains("teen")) classificacao = 14;
            else if (esrb.Contains("10+")) classificacao = 10;
            else if (esrb.Contains("everyone")) classificacao = 0;

            // 5. Descrição limpa
            var descricao = detail.DescriptionRaw;
            if (string.IsNullOrWhiteSpace(descricao) && !string.IsNullOrWhiteSpace(detail.DescriptionHtml))
            {
                descricao = Regex.Replace(detail.DescriptionHtml, "<.*?>", string.Empty).Trim();
            }
            if (string.IsNullOrWhiteSpace(descricao))
            {
                descricao = $"{tituloLimpo} é um aclamado jogo lançado em {dataLancamento.Year} pela {nomeEmpresa}.";
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
                Empresa = empresa,
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
                EmpresaId = j.Empresa?.Id ?? 0,
                NomeEmpresa = j.Empresa?.NomeEmpresa ?? string.Empty,
                EstaAtivo = j.EstaAtivo,
                Generos = j.Generos?.Select(g => g.TituloGenero).ToList() ?? new List<string>(),
                MediaAvaliacoes = null,
                TotalAvaliacoes = 0
            };
        }
    }
}
