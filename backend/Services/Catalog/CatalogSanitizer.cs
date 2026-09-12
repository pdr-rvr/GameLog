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

            // Limpar sufixos de edições redundantes para evitar entradas duplicadas do mesmo jogo
            lower = Regex.Replace(lower, @"\(\d{4}\)", "");
            lower = Regex.Replace(lower, @"\b(game of the year edition|goty edition|definitive edition|enhanced edition|remastered|remake|goodies collection|digital deluxe edition|special edition|anniversary edition|legendary edition|complete edition|directors cut|director's cut|hd remaster|deluxe edition|gold edition|collector's edition|collectors edition|anthology|trilogy|compilation|two-pack|double pack|edition)\b", "");

            // Remover caracteres especiais e espaços
            return Regex.Replace(lower, @"[^a-z0-9]", "");
        }

        public static string SanitizarTextoDescricaoHtml(string? html, string? tituloJogo = null, string? nomeEmpresa = null, int? anoLancamento = null)
        {
            return LimparDescricaoHtml(html, tituloJogo, nomeEmpresa, anoLancamento);
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
