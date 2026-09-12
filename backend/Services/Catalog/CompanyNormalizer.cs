using System;
using System.Collections.Generic;

namespace GameLog_Backend.Services.Catalog
{
    public static class CompanyNormalizer
    {
        private static readonly Dictionary<string, string> NormalizedCompanies = new(StringComparer.OrdinalIgnoreCase)
        {
            // Sony / PlayStation Studios
            { "Sony Interactive Entertainment", "PlayStation Studios" },
            { "Sony Computer Entertainment", "PlayStation Studios" },
            { "Sony Interactive", "PlayStation Studios" },
            { "PlayStation PC LLC", "PlayStation Studios" },
            { "Sony", "PlayStation Studios" },
            { "Santa Monica Studio", "PlayStation Studios" },
            { "Naughty Dog", "PlayStation Studios" },
            { "Insomniac Games", "PlayStation Studios" },
            { "Guerrilla Games", "PlayStation Studios" },
            { "Sucker Punch Productions", "PlayStation Studios" },
            { "Polyphony Digital", "PlayStation Studios" },

            // Xbox / Microsoft
            { "Microsoft Studios", "Xbox Game Studios" },
            { "Xbox Game Studios", "Xbox Game Studios" },
            { "Microsoft Corporation", "Xbox Game Studios" },
            { "343 Industries", "Xbox Game Studios" },
            { "Halo Studios", "Xbox Game Studios" },
            { "The Coalition", "Xbox Game Studios" },
            { "Turn 10 Studios", "Xbox Game Studios" },
            { "Playground Games", "Xbox Game Studios" },
            { "Rare Ltd.", "Xbox Game Studios" },
            { "Rare", "Xbox Game Studios" },
            { "Obsidian Entertainment", "Xbox Game Studios" },
            { "Ninja Theory", "Xbox Game Studios" },
            { "InXile Entertainment", "Xbox Game Studios" },

            // Nintendo
            { "Nintendo EPD", "Nintendo" },
            { "Nintendo of America", "Nintendo" },
            { "Nintendo of Europe", "Nintendo" },
            { "Monolith Soft", "Nintendo" },
            { "Game Freak", "Nintendo" },
            { "Creatures Inc.", "Nintendo" },
            { "HAL Laboratory", "Nintendo" },
            { "Intelligent Systems", "Nintendo" },

            // Electronic Arts
            { "Electronic Arts", "Electronic Arts" },
            { "EA Games", "Electronic Arts" },
            { "EA Sports", "Electronic Arts" },
            { "BioWare", "Electronic Arts" },
            { "Respawn Entertainment", "Electronic Arts" },
            { "DICE", "Electronic Arts" },
            { "Criterion Games", "Electronic Arts" },
            { "Maxis", "Electronic Arts" },

            // CD Projekt
            { "CD Projekt Red", "CD Projekt Red" },
            { "CD Projekt", "CD Projekt Red" },

            // Capcom
            { "Capcom", "Capcom" },
            { "Capcom U.S.A.", "Capcom" },
            { "Capcom Co., Ltd.", "Capcom" },

            // Square Enix
            { "Square Enix", "Square Enix" },
            { "Square", "Square Enix" },
            { "Squaresoft", "Square Enix" },
            { "Enix", "Square Enix" },

            // FromSoftware
            { "FromSoftware, Inc.", "FromSoftware" },
            { "FromSoftware", "FromSoftware" },
            { "From Software", "FromSoftware" },

            // Rockstar
            { "Rockstar Games", "Rockstar Games" },
            { "Rockstar North", "Rockstar Games" },
            { "Rockstar San Diego", "Rockstar Games" },

            // Ubisoft
            { "Ubisoft", "Ubisoft" },
            { "Ubisoft Entertainment", "Ubisoft" },
            { "Ubisoft Montreal", "Ubisoft" },
            { "Ubisoft Quebec", "Ubisoft" },

            // Bethesda & Zenimax
            { "Bethesda Softworks", "Bethesda Softworks" },
            { "Bethesda Game Studios", "Bethesda Softworks" },
            { "id Software", "Bethesda Softworks" },
            { "Arkane Studios", "Bethesda Softworks" },
            { "MachineGames", "Bethesda Softworks" },
            { "Tango Gameworks", "Bethesda Softworks" },

            // Bandai Namco
            { "Bandai Namco Entertainment", "Bandai Namco Entertainment" },
            { "Namco Bandai Games", "Bandai Namco Entertainment" },
            { "Bandai", "Bandai Namco Entertainment" },
            { "Namco", "Bandai Namco Entertainment" },

            // SEGA / Atlus
            { "SEGA", "SEGA" },
            { "Atlus", "SEGA" },
            { "Ryu Ga Gotoku Studio", "SEGA" },
            { "Creative Assembly", "SEGA" },

            // Konami
            { "Konami", "Konami" },
            { "Konami Digital Entertainment", "Konami" },

            // Valve
            { "Valve", "Valve" },
            { "Valve Corporation", "Valve" },

            // Blizzard / Activision
            { "Blizzard Entertainment", "Blizzard Entertainment" },
            { "Activision", "Activision" },
            { "Activision Blizzard", "Activision" },
            { "Infinity Ward", "Activision" },
            { "Treyarch", "Activision" },
            { "Sledgehammer Games", "Activision" },

            // Warner Bros
            { "Warner Bros. Games", "Warner Bros. Games" },
            { "Warner Bros. Interactive Entertainment", "Warner Bros. Games" },
            { "Rocksteady Studios", "Warner Bros. Games" },
            { "NetherRealm Studios", "Warner Bros. Games" },
            { "Monolith Productions", "Warner Bros. Games" },
            { "Avalanche Software", "Warner Bros. Games" },

            // 2K / Take-Two
            { "2K", "2K Games" },
            { "2K Games", "2K Games" },
            { "Firaxis Games", "2K Games" },
            { "Hangar 13", "2K Games" },
            { "Gearbox Software", "2K Games" }
        };

        public static string NormalizarNomeEmpresa(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return "Independente";
            var trim = nome.Trim();

            if (NormalizedCompanies.TryGetValue(trim, out var canonica))
            {
                return canonica;
            }

            // Normalização por aproximação
            var lower = trim.ToLowerInvariant();
            if (lower.Contains("sony") || lower.Contains("playstation") || lower.Contains("naughty dog") || lower.Contains("santa monica"))
                return "PlayStation Studios";
            if (lower.Contains("microsoft") || lower.Contains("xbox"))
                return "Xbox Game Studios";
            if (lower.Contains("nintendo") || lower.Contains("game freak"))
                return "Nintendo";
            if (lower.Contains("electronic arts") || lower.Contains("ea sports") || lower.Contains("bioware") || lower.Contains("respawn"))
                return "Electronic Arts";
            if (lower.Contains("cd projekt"))
                return "CD Projekt Red";
            if (lower.Contains("capcom"))
                return "Capcom";
            if (lower.Contains("square enix") || lower.Contains("squaresoft"))
                return "Square Enix";
            if (lower.Contains("fromsoftware") || lower.Contains("from software"))
                return "FromSoftware";
            if (lower.Contains("rockstar"))
                return "Rockstar Games";
            if (lower.Contains("ubisoft"))
                return "Ubisoft";
            if (lower.Contains("bethesda") || lower.Contains("id software") || lower.Contains("arkane"))
                return "Bethesda Softworks";
            if (lower.Contains("bandai namco") || lower.Contains("namco"))
                return "Bandai Namco Entertainment";
            if (lower.Contains("sega") || lower.Contains("atlus"))
                return "SEGA";
            if (lower.Contains("konami"))
                return "Konami";
            if (lower.Contains("valve"))
                return "Valve";
            if (lower.Contains("blizzard"))
                return "Blizzard Entertainment";
            if (lower.Contains("activision"))
                return "Activision";
            if (lower.Contains("warner bros") || lower.Contains("rocksteady"))
                return "Warner Bros. Games";
            if (lower.Contains("2k games") || lower.Contains("firaxis") || lower.Contains("gearbox"))
                return "2K Games";

            return trim;
        }

        public static string? ResolverEmpresaPorFranquia(string? titulo)
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
                return "PlayStation Studios";

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

        public static (string Desenvolvedora, string? Publicadora) ResolverParDesenvolvedoraPublicadora(string titulo, string? devAtual, string? pubAtual)
        {
            return Helpers.NormalizadorEmpresaHelper.ResolverParDesenvolvedoraPublicadora(titulo, devAtual, pubAtual);
        }
    }
}
