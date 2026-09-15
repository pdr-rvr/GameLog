using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameLog_Backend.Services.Catalog
{
    public static class CatalogSanitizer
    {
        private static readonly HashSet<string> InvalidTagSlugs = new(StringComparer.OrdinalIgnoreCase)
        {
            "fangame", "fan-game", "demake", "dlc", "addon", "add-on",
            "soundtrack", "expansion", "gameport", "romhack", "rpg-maker", "scratch"
        };

        private static readonly string[] DlcSubtitles = new[]
        {
            "crown of the sunken king", "crown of the old iron king", "crown of the ivory king",
            "artorias of the abyss", "the old hunters", "valhalla",
            "blood and wine", "hearts of stone", "the ringed city", "ashes of ariandel",
            "shadows of rose", "phantom liberty", "left behind", "shadow of the erdtree",
            "iceborne", "sunbreak", "separate ways", "the frozen wilds", "burning shores",
            "iki island", "burial at sea", "minerva's den", "dragonborn", "dawnguard",
            "hearthfire", "nuka-world", "far harbor", "automatron", "vault-tec workshop",
            "lonesome road", "old world blues", "honest hearts", "dead money", "point lookout",
            "broken steel", "the pitt", "mothership zeta", "operation: anchorage", "curse of the pharaohs",
            "the hidden ones", "legacy of the first blade", "the fate of atlantis", "dawn of ragnarok",
            "undead nightmare", "harley quinn's revenge", "cold, cold heart", "a matter of family",
            "the delicious last course", "whistleblower", "the signal", "the writer", "night springs",
            "the lake house", "ancient gods", "end of zoe", "not a hero", "the consequence",
            "the assignment", "the executioner", "freedom cry", "knife of dunwall", "the brigmore witches",
            "the missing link", "commander lilith", "sinclair solutions", "the lost artifact",
            "gathering storm", "rise and fall", "the grim and the grave", "call of the beastmen",
            "banned footage", "watchpoint pack", "gage historical", "reverse cosplay", "element of destruction",
            "zinyak attack", "space pack", "yokohama massage", "expedition", "lost between worlds",
            "ghosts - invasion", "ghosts - onslaught", "ghosts - devastation", "ghosts - nemesis",
            "first strike", "rezurrection", "all-in-one package", "intermission", "episode intermission",
            "future connected", "future redeemed", "torna the golden country", "torna - the golden country",
            "episode prompto", "episode gladiolus", "episode ignis", "episode ardyn",
            "jack the ripper", "dead kings", "a criminal past", "system rift", "trespasser",
            "jaws of hakkon", "the descent", "lair of the shadow broker", "arrival", "leviathan",
            "citadel", "omega", "captain scarlett", "mr. torgue", "tiny tina's assault",
            "bounty of blood", "moxxi's heist", "guns, love, and tentacles", "psycho krieg",
            "awe", "the foundation", "side effects", "the price of neutrality", "songs of the past"
        };

        public static bool EhJogoValido(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return false;
            var lower = nome.ToLowerInvariant().Trim();

            if (Regex.IsMatch(lower, @"#\w+")) return false;

            // 1. DLCs, expansões, demos, betas, benchmarks e game jams
            if (Regex.IsMatch(lower, @"\b(dlc|dlcs|expansion|expansions|soundtrack|soundtracks|score|season pass|expansion pass|battle pass|access pass|booster pack|booster|bonus content|deluxe upgrade|wallpaper engine|wallpaper|3dmark|soundpad|software|pre-order|free trial|playtest|closed beta|open beta|\bbeta\b|\balpha\b|prologue|demo|teaser|trailer|benchmark|test build|fanmade|fan game|fan-made|fangame|tribute|demake|game jam|gamejam|game clone|mod pack|blockout|toolkit|redkit|trainer|cheat engine|cheat)\b") ||
                Regex.IsMatch(lower, @"(^|\s|\()ost(\s|\)|$)") ||
                Regex.IsMatch(lower, @"(^|\s|\()demo(\s|\)|$)") ||
                Regex.IsMatch(lower, @"\b(remix\s*\(|fanmade\s+boss)\b"))
            {
                return false;
            }

            // 2. Edições cosméticas / duplicadas
            if (Regex.IsMatch(lower, @"\b(deluxe edition|digital deluxe|digital deluxe edition|premium edition|collector's edition|collectors edition|gold edition|ultimate edition|day one edition|launch edition|founder's pack|founders pack|champion edition|soundtrack edition|bonus edition|pre-order bonus|game of the year edition|goty edition|\bgoty\b|complete edition|definitive edition|royal edition|legacy edition|supreme edition|hero edition|vanguard edition|limited edition|special edition|standard edition)\b"))
            {
                return false;
            }

            // 3. Pacotes, Bundles e Multi-Packs
            if (Regex.IsMatch(lower, @"\b(pack|packs|bundle|bundles|two-pack|2-pack|double pack|triple pack|mission pack|upgrade pack|starter pack|character pack|skin pack|voice pack|costume pack|texture pack|item pack|clothing pack|weapon pack|bonus pack|item set|map pack)\b"))
            {
                return false;
            }

            // 4. Versões regionais duplicadas
            if (Regex.IsMatch(lower, @"\b(german edition|russian edition|french edition|spanish edition|japanese edition|chinese edition|italian edition|english edition|us edition|uk edition|pal version|ntsc version)\b"))
            {
                return false;
            }

            // 5. Episódios avulsos (preservando Half-Life 2, Star Wars e Sonic)
            if (Regex.IsMatch(lower, @"\b(episode\s+[0-9]+|ep\.\s*[0-9]+|chapter\s+[0-9]+)\b") &&
                !lower.Contains("half-life 2: episode") &&
                !lower.Contains("star wars episode") &&
                !lower.Contains("sonic the hedgehog 4"))
            {
                return false;
            }

            // 6. Subtítulos específicos de DLCs famosas
            foreach (var sub in DlcSubtitles)
            {
                if (lower.Contains(sub, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // 7. Protótipos, mods e fangames não-oficiais
            if (lower.Contains("itch.io") ||
                lower.Contains("(itch)") ||
                lower.Contains("notavirus") ||
                lower.Contains(".exe") ||
                lower.Contains(".apk") ||
                lower.Contains("chromebook") ||
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
                lower.Contains("... but ...") ||
                lower.Contains("boss fight") ||
                lower.Contains("bark souls") ||
                lower.Contains("ennard edition") ||
                lower.Contains("daughters of ash") ||
                (lower.Contains("nightfall") && lower.Contains("dark souls")) ||
                lower.Contains("tomb raider: catalyst") ||
                lower.Contains("osiris reborn") ||
                lower.Contains("resynced") ||
                lower.Contains("scars of kosmora") ||
                lower.Contains("children of the leaf") ||
                lower.Contains("pocket waifu") ||
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

        public static bool EhTagInvalida(string tagSlug)
        {
            if (string.IsNullOrWhiteSpace(tagSlug)) return false;
            return InvalidTagSlugs.Contains(tagSlug.Trim());
        }

        public static string LimparDescricaoHtml(string? html, string? tituloJogo = null, string? nomeEmpresa = null, int? anoLancamento = null)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return GerarSinopsePadrao(tituloJogo, nomeEmpresa, anoLancamento);
            }

            // Remove tags HTML
            var textoLimpo = Regex.Replace(html, "<.*?>", string.Empty);
            // Decodifica entidades comuns
            textoLimpo = textoLimpo.Replace("&nbsp;", " ")
                                   .Replace("&quot;", "\"")
                                   .Replace("&amp;", "&")
                                   .Replace("&lt;", "<")
                                   .Replace("&gt;", ">")
                                   .Replace("&#39;", "'");

            // Remove links, URLs ou menções de lojas
            textoLimpo = Regex.Replace(textoLimpo, @"https?://[^\s]+", string.Empty);
            textoLimpo = Regex.Replace(textoLimpo, @"\s+", " ").Trim();

            if (textoLimpo.Length < 15)
            {
                return GerarSinopsePadrao(tituloJogo, nomeEmpresa, anoLancamento);
            }

            if (textoLimpo.Length > 2000)
            {
                textoLimpo = textoLimpo.Substring(0, 2000).Trim() + "...";
            }

            return textoLimpo;
        }

        public static string NormalizarTituloParaDeduplicacao(string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;
            var lower = title.ToLowerInvariant().Trim();

            // Limpar sufixos de edições cosméticas e marketing redundantes para evitar duplicatas superficiais do mesmo jogo
            lower = Regex.Replace(lower, @"\(\d{4}\)", "");
            lower = Regex.Replace(lower, @"\b(game of the year edition|goty edition|goodies collection|digital deluxe edition|digital deluxe|special edition|anniversary edition|legendary edition|complete edition|directors cut|director's cut|deluxe edition|gold edition|collector's edition|collectors edition|anthology|two-pack|double pack|launch edition|day one edition|standard edition|founder's pack|founders pack)\b", "");

            // Remover caracteres especiais e espaços
            return Regex.Replace(lower, @"[^a-z0-9]", "");
        }

        public static string SanitizarTextoDescricaoHtml(string? html, string? tituloJogo = null, string? nomeEmpresa = null, int? anoLancamento = null)
        {
            return LimparDescricaoHtml(html, tituloJogo, nomeEmpresa, anoLancamento);
        }

        public static string? ExtrairSteamAppId(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var match = Regex.Match(url, @"store\.steampowered\.com/app/(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public static string SanitizarUrlCapa(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "/game-images/default_game_cover.png";
            var clean = url.Trim();

            // Se for URL da RAWG com crop forçado de baixa resolução, restaurar para resolução original
            if (clean.Contains("media.rawg.io/media/crop/"))
            {
                clean = Regex.Replace(clean, @"media\.rawg\.io/media/crop/\d+/\d+/", "media.rawg.io/media/");
            }

            return clean;
        }

        private static readonly Dictionary<string, string> CuratedMasterpieceCovers = new(StringComparer.OrdinalIgnoreCase)
        {
            // Top PC & Multiplatform Masterpieces (Steam 600x900 2x HD Boxarts Verificados)
            { "the witcher 3: wild hunt", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/292030/library_600x900_2x.jpg" },
            { "the witcher 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/292030/library_600x900_2x.jpg" },
            { "cyberpunk 2077", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1091500/library_600x900_2x.jpg" },
            { "elden ring: nightreign", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2622380/library_600x900_2x.jpg" },
            { "elden ring nightreign", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2622380/library_600x900_2x.jpg" },
            { "elden ring nightrain", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2622380/library_600x900_2x.jpg" },
            { "elden ring", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1245620/library_600x900_2x.jpg" },
            { "dark souls: remastered", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/570940/library_600x900_2x.jpg" },
            { "dark souls ii: scholar of the first sin", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/335300/library_600x900_2x.jpg" },
            { "dark souls iii", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/374320/library_600x900_2x.jpg" },
            { "sekiro: shadows die twice", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/814380/library_600x900_2x.jpg" },
            { "grand theft auto vi", "https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg" },
            { "gta vi", "https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg" },
            { "grand theft auto 6", "https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg" },
            { "gta 6", "https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg" },
            { "grand theft auto v", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/271590/library_600x900_2x.jpg" },
            { "gta v", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/271590/library_600x900_2x.jpg" },
            { "grand theft auto 5", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/271590/library_600x900_2x.jpg" },
            { "grand theft auto iv", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12210/library_600x900_2x.jpg" },
            { "gta iv", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12210/library_600x900_2x.jpg" },
            { "grand theft auto 4", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12210/library_600x900_2x.jpg" },
            { "grand theft auto: san andreas", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12120/library_600x900_2x.jpg" },
            { "grand theft auto: vice city", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12110/library_600x900_2x.jpg" },
            { "grand theft auto iii", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12100/library_600x900_2x.jpg" },
            { "grand theft auto 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12100/library_600x900_2x.jpg" },
            { "red dead redemption 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1174180/library_600x900_2x.jpg" },
            { "red dead redemption", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2668510/library_600x900_2x.jpg" },
            { "baldur's gate 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1086940/library_600x900_2x.jpg" },
            { "god of war (2018)", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1593500/library_600x900_2x.jpg" },
            { "half-life: alyx", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/546560/library_600x900_2x.jpg" },
            { "half-life 2: episode one", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/380/library_600x900_2x.jpg" },
            { "half-life 2: episode two", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/420/library_600x900_2x.jpg" },
            { "half-life 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/220/library_600x900_2x.jpg" },
            { "half-life", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/70/library_600x900_2x.jpg" },
            { "portal 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/620/library_600x900_2x.jpg" },
            { "portal", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/400/library_600x900_2x.jpg" },
            { "monster hunter: world", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/582010/library_600x900_2x.jpg" },
            { "hollow knight: silksong", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1030300/library_600x900_2x.jpg" },
            { "hollow knight", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/367520/library_600x900_2x.jpg" },
            { "hades ii", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145350/library_600x900_2x.jpg" },
            { "hades 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145350/library_600x900_2x.jpg" },
            { "hades", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1145360/library_600x900_2x.jpg" },
            { "persona 5 royal", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1687950/library_600x900_2x.jpg" },
            { "resident evil 4", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2050650/library_600x900_2x.jpg" },
            { "resident evil 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/883710/library_600x900_2x.jpg" },
            { "resident evil village", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1196590/library_600x900_2x.jpg" },
            { "resident evil 7 biohazard", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/418370/library_600x900_2x.jpg" },
            { "doom eternal", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/782330/library_600x900_2x.jpg" },
            { "nier:automata", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/524220/library_600x900_2x.jpg" },
            { "marvel's spider-man remastered", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1817070/library_600x900_2x.jpg" },
            { "street fighter 6", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1364780/library_600x900_2x.jpg" },
            { "tekken 8", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1778820/library_600x900_2x.jpg" },
            { "the elder scrolls v: skyrim", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/489830/library_600x900_2x.jpg" },
            { "the elder scrolls iv: oblivion", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/22330/library_600x900_2x.jpg" },
            { "the elder scrolls iii: morrowind", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/22320/library_600x900_2x.jpg" },
            { "fallout 4", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/377160/library_600x900_2x.jpg" },
            { "fallout: new vegas", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/22380/library_600x900_2x.jpg" },
            { "fallout 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/22370/library_600x900_2x.jpg" },
            { "fallout", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/38400/library_600x900_2x.jpg" },
            { "mass effect legendary edition", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1328670/library_600x900_2x.jpg" },
            { "max payne 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/204100/library_600x900_2x.jpg" },
            { "max payne 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12150/library_600x900_2x.jpg" },
            { "max payne", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/12140/library_600x900_2x.jpg" },
            { "final fantasy vii remake intergrade", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1462040/library_600x900_2x.jpg" },
            { "final fantasy vii", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/39140/library_600x900_2x.jpg" },
            { "final fantasy vi", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1173820/library_600x900_2x.jpg" },
            { "chrono trigger", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/613830/library_600x900_2x.jpg" },
            { "slay the spire 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2868840/library_600x900_2x.jpg" },
            { "slay the spire", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/646570/library_600x900_2x.jpg" },
            { "wreckfest 2", "https://media.rawg.io/media/screenshots/e5e/e5e61aefced41b79d41dac01664bc82e.jpg" },
            { "wreckfest", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/228380/library_600x900_2x.jpg" },
            { "pid", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/227860/library_600x900_2x.jpg" },
            { "sin", "https://media.rawg.io/media/screenshots/559/559f0bc2b44bc3223f14e4393a2f70d8.jpg" }
        };

        public static string ResolverMelhorCapaHd(
            string? rawgImageUrl, 
            IEnumerable<DTOs.RawgStoreItemDTO>? stores = null, 
            string? titulo = null, 
            int? anoLancamento = null,
            string? platformSlug = null)
        {
            // 1. Camada 1: Steam CDN Oficial Direta (600x900 2x Vertical)
            if (stores != null)
            {
                foreach (var store in stores)
                {
                    if (store == null) continue;
                    var slug = store.Store?.Slug?.ToLowerInvariant();
                    if (slug == "steam" || (!string.IsNullOrWhiteSpace(store.Url) && store.Url.Contains("steampowered.com")))
                    {
                        var appId = ExtrairSteamAppId(store.Url);
                        if (!string.IsNullOrWhiteSpace(appId))
                        {
                            return $"https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{appId}/library_600x900_2x.jpg";
                        }
                    }
                }
            }

            // 2. Camada 2: Tabela Curada de Clássicos, Exclusivos e Obras-Primas (600x900 HD)
            if (!string.IsNullOrWhiteSpace(titulo))
            {
                var titLower = titulo.Trim().ToLowerInvariant();
                if (CuratedMasterpieceCovers.TryGetValue(titLower, out var curatedUrl))
                {
                    return curatedUrl;
                }

                foreach (var kvp in CuratedMasterpieceCovers)
                {
                    if (Regex.IsMatch(titLower, $@"\b{Regex.Escape(kvp.Key)}\b", RegexOptions.IgnoreCase))
                    {
                        return kvp.Value;
                    }
                }
            }

            // 3. Camada 3: RAWG CDN Full-Res Sanitizada (sem crop de baixa resolução)
            return SanitizarUrlCapa(rawgImageUrl);
        }

        private static string GerarSinopsePadrao(string? titulo, string? empresa, int? ano)
        {
            var tit = !string.IsNullOrWhiteSpace(titulo) ? titulo : "Este título";
            var emp = !string.IsNullOrWhiteSpace(empresa) ? $" desenvolvido por {empresa}" : "";
            var anoStr = ano.HasValue ? $" em {ano.Value}" : "";

            return $"{tit} é um jogo aclamado{emp}{anoStr}, apresentando uma experiência envolvente com jogabilidade refinada e rica ambientação.";
        }
    }
}
