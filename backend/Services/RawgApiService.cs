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
using GameLog_Backend.Helpers;
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

                // Filtrar DLCs, itens inválidos e títulos irrelevantes à busca
                var jogosValidos = response.Results
                    .Where(r => EhJogoValido(r.Name) && RelevanciaBuscaHelper.CorrespondeBusca(r.Name, null, termo))
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

        public static string MapearGeneroParaPortugues(string rawgGenre)
        {
            if (string.IsNullOrWhiteSpace(rawgGenre)) return "Ação";
            var slug = rawgGenre.Trim().ToLowerInvariant();

            if (slug.Contains("action") || slug.Contains("acao") || slug.Contains("aao")) return "Ação";
            if (slug.Contains("adventure") || slug.Contains("aventura")) return "Aventura";
            if (slug.Contains("role-playing") || slug.Contains("rpg")) return "RPG";
            if (slug.Contains("shooter") || slug.Contains("tiro") || slug.Contains("fps")) return "Tiro";
            if (slug.Contains("strategy") || slug.Contains("estrategia") || slug.Contains("estratgia")) return "Estratégia";
            if (slug.Contains("racing") || slug.Contains("corrida")) return "Corrida";
            if (slug.Contains("sports") || slug.Contains("esportes")) return "Esportes";
            if (slug.Contains("fighting") || slug.Contains("luta")) return "Luta";
            if (slug.Contains("puzzle") || slug.Contains("quebra-cabeca") || slug.Contains("quebra-cabea")) return "Quebra-Cabeça";
            if (slug.Contains("simulation") || slug.Contains("simulacao") || slug.Contains("simulaao")) return "Simulação";
            if (slug.Contains("horror") || slug.Contains("survival") || slug.Contains("terror")) return "Terror e Sobrevivência";
            if (slug.Contains("platform") || slug.Contains("plataforma")) return "Plataforma";
            if (slug.Contains("open world") || slug.Contains("mundo aberto")) return "Mundo Aberto";
            if (slug.Contains("hack and slash") || slug.Contains("hack-and-slash")) return "Hack and Slash";
            if (slug.Contains("metroidvania")) return "Metroidvania";
            if (slug.Contains("roguelike") || slug.Contains("rogue-like")) return "Roguelike";
            if (slug.Contains("stealth")) return "Stealth";
            if (slug.Contains("indie")) return "Indie";
            if (slug.Contains("casual")) return "Casual";
            if (slug.Contains("arcade")) return "Arcade";
            if (slug.Contains("massively multiplayer") || slug.Contains("mmo")) return "MMO";

            return rawgGenre.Trim();
        }

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

        public static string? ResolverEmpresaPorFranquia(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return null;
            var t = titulo.ToLowerInvariant();

            if (t.Contains("zelda") || t.Contains("mario") || t.Contains("pokemon") || t.Contains("pokémon") ||
                t.Contains("metroid") || t.Contains("donkey kong") || t.Contains("kirby") || t.Contains("fire emblem") ||
                t.Contains("super smash") || t.Contains("splatoon") || t.Contains("xenoblade") || t.Contains("animal crossing") ||
                t.Contains("f-zero") || t.Contains("star fox") || t.Contains("luigi") || t.Contains("pikmin") || t.Contains("smash bros"))
                return "Nintendo";

            if (t.Contains("god of war") || t.Contains("uncharted") || t.Contains("the last of us") ||
                t.Contains("gran turismo") || t.Contains("horizon zero") || t.Contains("horizon forbidden") ||
                t.Contains("ghost of tsushima") || t.Contains("bloodborne") || t.Contains("killzone") ||
                t.Contains("infamous") || t.Contains("ratchet & clank") || t.Contains("spider-man"))
                return "Sony Interactive Entertainment";

            if (t.Contains("halo") || t.Contains("gears of war") || t.Contains("forza") || t.Contains("fable") ||
                t.Contains("age of empires") || t.Contains("sea of thieves") || t.Contains("banjo-kazooie"))
                return "Xbox Game Studios";

            if (t.Contains("grand theft auto") || t.Contains("gta") || t.Contains("red dead") ||
                t.Contains("max payne") || t.Contains("bully") || t.Contains("midnight club"))
                return "Rockstar Games";

            if (t.Contains("witcher") || t.Contains("cyberpunk 2077"))
                return "CD Projekt Red";

            if (t.Contains("resident evil") || t.Contains("monster hunter") || t.Contains("devil may cry") ||
                t.Contains("street fighter") || t.Contains("mega man") || t.Contains("dragons dogma") || t.Contains("dragon's dogma") ||
                t.Contains("ace attorney") || t.Contains("dead rising"))
                return "Capcom";

            if (t.Contains("final fantasy") || t.Contains("dragon quest") || t.Contains("kingdom hearts") ||
                t.Contains("chrono") || t.Contains("nier") || t.Contains("tomb raider") || t.Contains("deus ex"))
                return "Square Enix";

            if (t.Contains("dark souls") || t.Contains("elden ring") || t.Contains("sekiro") || t.Contains("armored core"))
                return "FromSoftware";

            if (t.Contains("assassin's creed") || t.Contains("far cry") || t.Contains("rainbow six") ||
                t.Contains("splinter cell") || t.Contains("ghost recon") || t.Contains("watch dogs") ||
                t.Contains("rayman") || t.Contains("prince of persia") || t.Contains("the division"))
                return "Ubisoft";

            if (t.Contains("fifa") || t.Contains("ea sports") || t.Contains("battlefield") ||
                t.Contains("mass effect") || t.Contains("dragon age") || t.Contains("need for speed") ||
                t.Contains("dead space") || t.Contains("the sims") || t.Contains("apex legends") || t.Contains("titanfall"))
                return "Electronic Arts";

            if (t.Contains("half-life") || t.Contains("portal") || t.Contains("left 4 dead") ||
                t.Contains("counter-strike") || t.Contains("team fortress") || t.Contains("dota"))
                return "Valve";

            if (t.Contains("elder scrolls") || t.Contains("skyrim") || t.Contains("fallout") ||
                t.Contains("doom") || t.Contains("dishonored") || t.Contains("wolfenstein") || t.Contains("quake") || t.Contains("prey"))
                return "Bethesda Softworks";

            if (t.Contains("metal gear") || t.Contains("silent hill") || t.Contains("castlevania") || t.Contains("pes ") || t.Contains("efootball"))
                return "Konami";

            if (t.Contains("sonic") || t.Contains("yakuza") || t.Contains("like a dragon") ||
                t.Contains("persona") || t.Contains("shin megami") || t.Contains("total war"))
                return "SEGA";

            if (t.Contains("tekken") || t.Contains("tales of") || t.Contains("naruto") || t.Contains("dragon ball") || t.Contains("pac-man"))
                return "Bandai Namco Entertainment";

            if (t.Contains("warcraft") || t.Contains("diablo") || t.Contains("starcraft") || t.Contains("overwatch"))
                return "Blizzard Entertainment";

            return null;
        }

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
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDTO<RawgGameItemDTO>>(url);
                if (response?.Results == null || !response.Results.Any())
                {
                    return new List<JogoDTO>();
                }

                var validos = response.Results
                    .Where(r => EhJogoValido(r.Name) 
                             && !string.IsNullOrWhiteSpace(r.BackgroundImage) 
                             && r.BackgroundImage.StartsWith("http")
                             && ((r.Added ?? 0) >= 10 || (r.RatingsCount ?? 0) >= 3 || r.Metacritic.HasValue)
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
                            EmpresaId = localJogo.Empresa?.Id ?? 0,
                            NomeEmpresa = localJogo.Empresa?.NomeEmpresa ?? string.Empty,
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

                        var nomeDev = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(devRaw ?? pubRaw);
                        var nomePub = !string.IsNullOrWhiteSpace(pubRaw) ? NormalizadorEmpresaHelper.NormalizarNomeEmpresa(pubRaw) : null;

                        var generosSource = (detail?.Genres != null && detail.Genres.Any()) ? detail.Genres : r.Genres;
                        var generosNormalizados = generosSource?
                            .Select(g => MapearGeneroParaPortugues(g.Name))
                            .Distinct()
                            .ToList() ?? new List<string>();

                        var classificacao = MapearEsrbParaClassificacao(
                            detail?.EsrbRating?.Slug ?? detail?.EsrbRating?.Name ?? r.EsrbRating?.Slug ?? r.EsrbRating?.Name
                        );

                        resultado.Add(new JogoDTO
                        {
                            JogoId = 0,
                            RawgId = r.Id,
                            Titulo = r.Name.Trim(),
                            Descricao = detail?.DescriptionRaw ?? string.Empty,
                            Imagem = detail?.BackgroundImage ?? r.BackgroundImage ?? "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&auto=format&fit=crop&q=80",
                            DataLancamento = dataLanc,
                            ClassificacaoIndicativa = classificacao,
                            EmpresaId = 0,
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
                _logger.LogWarning(ex, "Falha ao buscar jogos híbridos na RAWG para query '{Query}'", queryString);
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

            // 1. Obter ou Criar Desenvolvedora e Publicadora Canônicas
            var devRaw = detail.Developers.FirstOrDefault()?.Name ?? detail.Publishers.FirstOrDefault()?.Name;
            var pubRaw = detail.Publishers.FirstOrDefault()?.Name ?? ResolverEmpresaPorFranquia(detail.Name);

            var nomeDev = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(devRaw ?? pubRaw);
            var nomePub = !string.IsNullOrWhiteSpace(pubRaw) ? NormalizadorEmpresaHelper.NormalizarNomeEmpresa(pubRaw) : null;

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
                EmpresaId = j.Empresa?.Id ?? 0,
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
