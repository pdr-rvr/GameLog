using System;
using System.Collections.Generic;

namespace GameLog_Backend.Helpers
{
    public static class NormalizadorEmpresaHelper
    {
        // Mapeamento canônico exato (insensível a maiúsculas/minúsculas)
        private static readonly Dictionary<string, string> MapeamentoExato = new(StringComparer.OrdinalIgnoreCase)
        {
            // Sony / PlayStation
            { "Sony Interactive Entertainment", "PlayStation Studios" },
            { "Sony Computer Entertainment", "PlayStation Studios" },
            { "PlayStation Publishing", "PlayStation Studios" },
            { "PlayStation PC", "PlayStation Studios" },
            { "PlayStation Mobile", "PlayStation Studios" },
            { "Sony Interactive Entertainment Europe", "PlayStation Studios" },
            { "Sony Interactive Entertainment LLC", "PlayStation Studios" },
            { "Sony Computer Entertainment America", "PlayStation Studios" },
            { "Sony Pictures Digital", "PlayStation Studios" },
            { "Sony Online Entertainment", "PlayStation Studios" },
            { "Sony", "PlayStation Studios" },

            // Ubisoft
            { "Ubisoft Entertainment", "Ubisoft" },
            { "Ubisoft Montreal", "Ubisoft" },
            { "Ubisoft Paris", "Ubisoft" },
            { "Ubisoft Quebec", "Ubisoft" },
            { "Ubisoft Montpellier", "Ubisoft" },
            { "Ubisoft Toronto", "Ubisoft" },
            { "Ubisoft San Francisco", "Ubisoft" },
            { "Ubisoft Milan", "Ubisoft" },
            { "Ubisoft Reflections", "Ubisoft" },
            { "Ubisoft Sofia", "Ubisoft" },
            { "Ubisoft Kiev", "Ubisoft" },
            { "Ubisoft Shanghai", "Ubisoft" },

            // Bandai Namco
            { "Bandai Namco Entertainment", "Bandai Namco" },
            { "Bandai Namco Games", "Bandai Namco" },
            { "Namco Bandai Games", "Bandai Namco" },
            { "Namco", "Bandai Namco" },
            { "Bandai", "Bandai Namco" },
            { "Bandai Namco Studios", "Bandai Namco" },

            // Microsoft / Xbox
            { "Microsoft Studios", "Xbox Game Studios" },
            { "Microsoft Corporation", "Xbox Game Studios" },
            { "Microsoft Game Studios", "Xbox Game Studios" },
            { "Microsoft", "Xbox Game Studios" },
            { "Xbox Game Studios Publishing", "Xbox Game Studios" },

            // Square Enix
            { "Square", "Square Enix" },
            { "Enix", "Square Enix" },
            { "Square Enix Europe", "Square Enix" },
            { "Square Electronic Arts", "Square Enix" },
            { "Square Enix Montreal", "Square Enix" },
            { "Square Enix Ltd.", "Square Enix" },

            // Electronic Arts
            { "EA Games", "Electronic Arts" },
            { "EA Sports", "Electronic Arts" },
            { "EA Sports Big", "Electronic Arts" },
            { "EA Black Box", "Electronic Arts" },
            { "EA Redwood Shores", "Electronic Arts" },
            { "EA", "Electronic Arts" },

            // 2K / Take-Two
            { "2K Games", "2K" },
            { "2K Sports", "2K" },
            { "2K Marin", "2K" },
            { "2K Czech", "2K" },
            { "2K Play", "2K" },
            { "2K Boston", "2K" },
            { "Take-Two Interactive", "Take-Two" },

            // Warner Bros.
            { "Warner Bros. Interactive", "Warner Bros. Games" },
            { "Warner Bros. Interactive Entertainment", "Warner Bros. Games" },
            { "Warner Bros. Games Montreal", "Warner Bros. Games" },
            { "WB Games", "Warner Bros. Games" },
            { "Warner Bros.", "Warner Bros. Games" },

            // Valve
            { "Valve Corporation", "Valve" },

            // Bethesda
            { "Bethesda Game Studios", "Bethesda Softworks" },

            // THQ / Deep Silver
            { "Deep Silver / THQ Nordic", "THQ Nordic" },
            { "THQ Nordic Vienna", "THQ Nordic" },
            { "THQ", "THQ Nordic" },
            { "Deep Silver Volition", "Deep Silver" },
            { "Deep Silver Dambuster", "Deep Silver" },

            // Team17
            { "Team17 Digital", "Team17" },
            { "Team17 Software", "Team17" },
            { "Team17 Digital Limited", "Team17" },

            // Capcom
            { "Capcom U.S.A.", "Capcom" },
            { "Capcom U.S.A., Inc.", "Capcom" },
            { "Capcom Production Studio", "Capcom" },
            { "Capcom Entertainment", "Capcom" },

            // SEGA
            { "Sega", "SEGA" },
            { "SEGA of America", "SEGA" },
            { "SEGA Europe", "SEGA" },

            // Konami
            { "Konami Digital Entertainment", "Konami" },
            { "Konami Corporation", "Konami" },
            { "Konami Digital Entertainment, Inc.", "Konami" },

            // CD Projekt
            { "CD PROJEKT RED", "CD Projekt Red" },
            { "CD Projekt", "CD Projekt Red" },

            // Rockstar
            { "Rockstar North", "Rockstar Games" },
            { "Rockstar San Diego", "Rockstar Games" },
            { "Rockstar Leeds", "Rockstar Games" },
            { "Rockstar Toronto", "Rockstar Games" },
            { "Rockstar New England", "Rockstar Games" },

            // Nintendo
            { "Nintendo EPD", "Nintendo" },
            { "Nintendo SPD", "Nintendo" },
            { "Nintendo of America", "Nintendo" },
            { "Nintendo of Europe", "Nintendo" },
            { "Nintendo EAD", "Nintendo" },

            // Koei Tecmo
            { "Tecmo Koei", "Koei Tecmo" },
            { "Koei", "Koei Tecmo" },
            { "Tecmo", "Koei Tecmo" },
            { "Koei Tecmo Games", "Koei Tecmo" },

            // Focus
            { "Focus Home Interactive", "Focus Entertainment" },

            // Spike Chunsoft
            { "Chunsoft", "Spike Chunsoft" },
            { "Spike", "Spike Chunsoft" },
            { "Spike Chunsoft Co., Ltd.", "Spike Chunsoft" },

            // Curve
            { "Curve Digital", "Curve Games" },

            // 505 Games
            { "505 Games (US)", "505 Games" },

            // Annapurna
            { "Annapurna Games", "Annapurna Interactive" },

            // FromSoftware
            { "From Software", "FromSoftware" },
            { "FromSoftware, Inc.", "FromSoftware" },

            // Level-5
            { "Level 5", "Level-5" },

            // LucasArts
            { "LucasArts Entertainment", "LucasArts" }
        };

        public static string NormalizarNomeEmpresa(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return "Desenvolvedor Independente";

            var trimmed = nome.Trim();
            if (MapeamentoExato.TryGetValue(trimmed, out var canonico))
            {
                return canonico;
            }

            var lower = trimmed.ToLowerInvariant();

            // Regras heurísticas de consolidação por sufixo / prefixo
            if (lower.StartsWith("ubisoft ") || lower.Contains("ubisoft ent")) return "Ubisoft";
            if (lower.StartsWith("playstation ") || lower.StartsWith("sony interactive") || lower.StartsWith("sony computer")) return "PlayStation Studios";
            if (lower.StartsWith("microsoft ") || lower.Contains("xbox game studios")) return "Xbox Game Studios";
            if (lower.Contains("bandai namco") || lower.Contains("namco bandai")) return "Bandai Namco";
            if (lower.Contains("square enix")) return "Square Enix";
            if (lower.Contains("electronic arts") || lower.StartsWith("ea sports") || lower.StartsWith("ea games")) return "Electronic Arts";
            if (lower.StartsWith("warner bros") || lower.Contains("wb games")) return "Warner Bros. Games";
            if (lower.StartsWith("capcom ")) return "Capcom";
            if (lower.StartsWith("sega ") || lower == "sega") return "SEGA";
            if (lower.StartsWith("konami ")) return "Konami";
            if (lower.Contains("cd projekt")) return "CD Projekt Red";
            if (lower.StartsWith("rockstar ")) return "Rockstar Games";
            if (lower.StartsWith("nintendo ")) return "Nintendo";
            if (lower.StartsWith("team17 ")) return "Team17";
            if (lower.StartsWith("valve ")) return "Valve";
            if (lower.Contains("bethesda game studios")) return "Bethesda Softworks";
            if (lower.Contains("from software") || lower == "fromsoftware, inc.") return "FromSoftware";

            return trimmed;
        }

        public static (string Desenvolvedora, string? Publicadora) ResolverParDesenvolvedoraPublicadora(string titulo, string? devAtual, string? pubAtual)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return (devAtual ?? "Desenvolvedor Independente", pubAtual);
            var t = titulo.ToLowerInvariant();

            // FromSoftware
            if (t.Contains("elden ring") || t.Contains("dark souls") || t.Contains("armored core"))
                return ("FromSoftware", "Bandai Namco");
            if (t.Contains("sekiro"))
                return ("FromSoftware", "Activision");
            if (t.Contains("bloodborne") || t.Contains("demon's souls") || t.Contains("demons souls"))
                return ("FromSoftware", "PlayStation Studios");

            // PlayStation Studios First-Party
            if (t.Contains("god of war"))
                return ("Santa Monica Studio", "PlayStation Studios");
            if (t.Contains("the last of us") || t.Contains("uncharted") || t.Contains("jak and daxter"))
                return ("Naughty Dog", "PlayStation Studios");
            if (t.Contains("spider-man") || t.Contains("spiderman") || t.Contains("ratchet & clank") || t.Contains("ratchet and clank") || t.Contains("wolverine") || t.Contains("resistance:"))
                return ("Insomniac Games", "PlayStation Studios");
            if (t.Contains("horizon zero") || t.Contains("horizon forbidden") || t.Contains("killzone"))
                return ("Guerrilla Games", "PlayStation Studios");
            if (t.Contains("ghost of tsushima") || t.Contains("ghost of yotei") || t.Contains("infamous") || t.Contains("sly cooper"))
                return ("Sucker Punch Productions", "PlayStation Studios");
            if (t.Contains("gran turismo"))
                return ("Polyphony Digital", "PlayStation Studios");
            if (t.Contains("days gone"))
                return ("Bend Studio", "PlayStation Studios");
            if (t.Contains("returnal"))
                return ("Housemarque", "PlayStation Studios");
            if (t.Contains("death stranding"))
                return ("Kojima Productions", "PlayStation Studios");
            if (t.Contains("until dawn"))
                return ("Supermassive Games", "PlayStation Studios");
            if (t.Contains("the order: 1886") || t.Contains("the order 1886"))
                return ("Ready at Dawn", "PlayStation Studios");
            if (t.Contains("shadow of the colossus") || t.Contains("ico"))
                return ("Team ICO", "PlayStation Studios");
            if (t.Contains("astro bot") || t.Contains("astro's playroom"))
                return ("Team ASOBI", "PlayStation Studios");

            // Xbox Game Studios First-Party
            if (t.Contains("halo"))
                return (t.Contains("infinite") || t.Contains("halo 5") || t.Contains("halo 4") || t.Contains("master chief") ? "343 Industries" : "Bungie", "Xbox Game Studios");
            if (t.Contains("gears of war") || t.Contains("gears 5") || t.Contains("gears tactics"))
                return (t.Contains("4") || t.Contains("5") || t.Contains("tactics") || t.Contains("reloaded") || t.Contains("e-day") ? "The Coalition" : "Epic Games", "Xbox Game Studios");
            if (t.Contains("forza horizon"))
                return ("Playground Games", "Xbox Game Studios");
            if (t.Contains("forza motorsport"))
                return ("Turn 10 Studios", "Xbox Game Studios");
            if (t.Contains("fable"))
                return ("Lionhead Studios", "Xbox Game Studios");
            if (t.Contains("sea of thieves") || t.Contains("banjo-kazooie") || t.Contains("perfect dark"))
                return ("Rare", "Xbox Game Studios");
            if (t.Contains("hellblade"))
                return ("Ninja Theory", "Xbox Game Studios");
            if (t.Contains("psychonauts"))
                return ("Double Fine Productions", "Xbox Game Studios");
            if (t.Contains("state of decay"))
                return ("Undead Labs", "Xbox Game Studios");
            if (t.Contains("quantum break"))
                return ("Remedy Entertainment", "Xbox Game Studios");
            if (t.Contains("sunset overdrive"))
                return ("Insomniac Games", "Xbox Game Studios");

            // Bethesda Softworks
            if (t.Contains("fallout: new vegas") || t.Contains("fallout new vegas"))
                return ("Obsidian Entertainment", "Bethesda Softworks");
            if (t.Contains("the outer worlds") || t.Contains("avowed") || t.Contains("pillars of eternity") || t.Contains("pentiment") || t.Contains("grounded") || t.Contains("tyranny"))
                return ("Obsidian Entertainment", "Xbox Game Studios");
            if (t.Contains("skyrim") || t.Contains("elder scrolls") || t.Contains("oblivion") || t.Contains("morrowind") || t.Contains("fallout 3") || t.Contains("fallout 4") || t.Contains("fallout 76") || t.Contains("starfield"))
                return ("Bethesda Game Studios", "Bethesda Softworks");
            if (t.Contains("dishonored") || t.Contains("deathloop") || t.Contains("prey") || t.Contains("redfall") || t.Contains("blade"))
                return ("Arkane Studios", "Bethesda Softworks");
            if (t.Contains("doom") || t.Contains("rage ") || t.Contains("rage 2") || t.Contains("quake"))
                return ("id Software", "Bethesda Softworks");
            if (t.Contains("wolfenstein"))
                return ("MachineGames", "Bethesda Softworks");
            if (t.Contains("the evil within") || t.Contains("ghostwire: tokyo") || t.Contains("hi-fi rush"))
                return ("Tango Gameworks", "Bethesda Softworks");

            // Electronic Arts
            if (t.Contains("mass effect") || t.Contains("dragon age") || t.Contains("anthem") || t.Contains("jade empire") || t.Contains("neverwinter nights"))
                return ("BioWare", "Electronic Arts");
            if (t.Contains("battlefield") || t.Contains("star wars battlefront") || t.Contains("mirrors edge") || t.Contains("mirror's edge"))
                return ("DICE", "Electronic Arts");
            if (t.Contains("apex legends") || t.Contains("titanfall") || t.Contains("jedi: fallen") || t.Contains("jedi: survivor") || t.Contains("jedi fallen") || t.Contains("jedi survivor"))
                return ("Respawn Entertainment", "Electronic Arts");
            if (t.Contains("dead space"))
                return ("Visceral Games", "Electronic Arts");
            if (t.Contains("need for speed") || t.Contains("burnout"))
                return ("Criterion Games", "Electronic Arts");
            if (t.Contains("the sims") || t.Contains("simcity") || t.Contains("spore"))
                return ("Maxis", "Electronic Arts");
            if (t.Contains("it takes two") || t.Contains("a way out"))
                return ("Hazelight Studios", "Electronic Arts");

            // 2K Games
            if (t.Contains("bioshock"))
                return ("Irrational Games", "2K");
            if (t.Contains("borderlands") || t.Contains("tiny tina") || t.Contains("battleborn"))
                return ("Gearbox Software", "2K");
            if (t.Contains("civilization") || t.Contains("xcom") || t.Contains("midnight suns"))
                return ("Firaxis Games", "2K");
            if (t.Contains("mafia"))
                return ("Hangar 13", "2K");
            if (t.Contains("nba 2k") || t.Contains("wwe 2k"))
                return ("Visual Concepts", "2K");

            // Rockstar Games
            if (t.Contains("grand theft auto") || t.Contains("gta") || t.Contains("red dead") || t.Contains("bully") || t.Contains("manhunt") || t.Contains("midnight club"))
                return ("Rockstar North", "Rockstar Games");
            if (t.Contains("max payne 3"))
                return ("Rockstar Studios", "Rockstar Games");
            if (t.Contains("max payne") && !t.Contains("3"))
                return ("Remedy Entertainment", "Rockstar Games");
            if (t.Contains("l.a. noire") || t.Contains("la noire"))
                return ("Team Bondi", "Rockstar Games");

            // Warner Bros. Games
            if (t.Contains("batman: arkham asylum") || t.Contains("batman: arkham city") || t.Contains("batman: arkham knight") || t.Contains("suicide squad"))
                return ("Rocksteady Studios", "Warner Bros. Games");
            if (t.Contains("batman: arkham origins") || t.Contains("gotham knights"))
                return ("WB Games Montreal", "Warner Bros. Games");
            if (t.Contains("mortal kombat") || t.Contains("injustice"))
                return ("NetherRealm Studios", "Warner Bros. Games");
            if (t.Contains("hogwarts legacy"))
                return ("Avalanche Software", "Warner Bros. Games");
            if (t.Contains("shadow of mordor") || t.Contains("shadow of war") || t.Contains("f.e.a.r."))
                return ("Monolith Productions", "Warner Bros. Games");
            if (t.Contains("hitman 2") || t.Contains("hitman 2018"))
                return ("IO Interactive", "Warner Bros. Games");

            // SEGA
            if (t.Contains("persona") || t.Contains("shin megami") || t.Contains("metaphor: refantazio") || t.Contains("catherine"))
                return ("Atlus", "SEGA");
            if (t.Contains("yakuza") || t.Contains("like a dragon") || t.Contains("judgment") || t.Contains("lost judgment"))
                return ("Ryu Ga Gotoku Studio", "SEGA");
            if (t.Contains("total war"))
                return ("Creative Assembly", "SEGA");
            if (t.Contains("sonic"))
                return ("Sonic Team", "SEGA");
            if (t.Contains("bayonetta") && !t.Contains("2") && !t.Contains("3"))
                return ("PlatinumGames", "SEGA");
            if (t.Contains("alien: isolation") || t.Contains("alien isolation"))
                return ("Creative Assembly", "SEGA");
            if (t.Contains("two point"))
                return ("Two Point Studios", "SEGA");

            // Square Enix
            if (t.Contains("nier:automata") || t.Contains("nier automata") || t.Contains("nier replicant"))
                return ("PlatinumGames", "Square Enix");
            if (t.Contains("tomb raider") || t.Contains("rise of the tomb") || t.Contains("shadow of the tomb") || t.Contains("legacy of kain"))
                return ("Crystal Dynamics", "Square Enix");
            if (t.Contains("deus ex"))
                return ("Eidos Montreal", "Square Enix");
            if (t.Contains("life is strange") && (t.Contains("2") || t.Contains("1") || t.Contains("remastered") || !t.Contains("true colors")))
                return ("DON'T NOD", "Square Enix");
            if (t.Contains("life is strange: true colors") || t.Contains("life is strange: before the storm") || t.Contains("life is strange: double exposure"))
                return ("Deck Nine", "Square Enix");
            if (t.Contains("just cause") || t.Contains("mad max"))
                return ("Avalanche Studios", "Square Enix");
            if (t.Contains("octopath traveler") || t.Contains("triangle strategy") || t.Contains("bravely default"))
                return ("Acquire", "Square Enix");
            if (t.Contains("final fantasy") || t.Contains("dragon quest") || t.Contains("kingdom hearts") || t.Contains("chrono") || t.Contains("mana"))
                return ("Square Enix", "Square Enix");

            // Nintendo
            if (t.Contains("pokémon") || t.Contains("pokemon"))
                return ("Game Freak", "Nintendo");
            if (t.Contains("super smash bros") || t.Contains("smash bros"))
                return ("Bandai Namco", "Nintendo");
            if (t.Contains("kirby"))
                return ("HAL Laboratory", "Nintendo");
            if (t.Contains("fire emblem") || t.Contains("paper mario") || t.Contains("warioware"))
                return ("Intelligent Systems", "Nintendo");
            if (t.Contains("xenoblade"))
                return ("Monolith Soft", "Nintendo");
            if (t.Contains("metroid prime") || t.Contains("donkey kong country: tropical"))
                return ("Retro Studios", "Nintendo");
            if (t.Contains("metroid dread") || t.Contains("metroid: samus returns"))
                return ("MercurySteam", "Nintendo");
            if (t.Contains("luigi's mansion 3") || t.Contains("mario strikers"))
                return ("Next Level Games", "Nintendo");
            if (t.Contains("bayonetta 2") || t.Contains("bayonetta 3") || t.Contains("astral chain"))
                return ("PlatinumGames", "Nintendo");
            if (t.Contains("zelda") || t.Contains("mario") || t.Contains("splatoon") || t.Contains("animal crossing") || t.Contains("pikmin") || t.Contains("star fox") || t.Contains("f-zero"))
                return ("Nintendo", "Nintendo");

            // Remedy Entertainment
            if (t.Contains("alan wake 2") || t.Contains("alan wake ii"))
                return ("Remedy Entertainment", "Epic Games");
            if (t.Contains("control"))
                return ("Remedy Entertainment", "505 Games");
            if (t.Contains("alan wake"))
                return ("Remedy Entertainment", "Xbox Game Studios");

            // Capcom
            if (t.Contains("resident evil") || t.Contains("monster hunter") || t.Contains("devil may cry") || t.Contains("street fighter") || t.Contains("mega man") || t.Contains("dragons dogma") || t.Contains("dragon's dogma") || t.Contains("ace attorney") || t.Contains("dead rising"))
                return ("Capcom", "Capcom");

            // Bandai Namco
            if (t.Contains("tekken") || t.Contains("tales of") || t.Contains("naruto") || t.Contains("dragon ball") || t.Contains("pac-man"))
                return ("Bandai Namco", "Bandai Namco");

            // Konami
            if (t.Contains("metal gear") || t.Contains("silent hill") || t.Contains("castlevania") || t.Contains("pes ") || t.Contains("efootball"))
                return ("Konami", "Konami");

            // Blizzard
            if (t.Contains("warcraft") || t.Contains("diablo") || t.Contains("starcraft") || t.Contains("overwatch"))
                return ("Blizzard Entertainment", "Activision Blizzard");

            // Ubisoft
            if (t.Contains("assassin's creed") || t.Contains("far cry") || t.Contains("rainbow six") || t.Contains("splinter cell") || t.Contains("ghost recon") || t.Contains("watch dogs") || t.Contains("rayman") || t.Contains("prince of persia") || t.Contains("the division") || t.Contains("skull and bones") || t.Contains("skull & bones") || t.Contains("star wars outlaws") || t.Contains("avatar: frontiers"))
                return ("Ubisoft", "Ubisoft");

            // Indies e Notáveis
            if (t.Contains("outer wilds"))
                return ("Mobius Digital", "Annapurna Interactive");
            if (t.Contains("stray"))
                return ("BlueTwelve Studio", "Annapurna Interactive");
            if (t.Contains("what remains of edith finch"))
                return ("Giant Sparrow", "Annapurna Interactive");
            if (t.Contains("neon white"))
                return ("Angel Matrix", "Annapurna Interactive");
            if (t.Contains("tunic"))
                return ("Andrew Shouldice", "Finji");
            if (t.Contains("sifu"))
                return ("Sloclap", "Kepler Interactive");
            if (t.Contains("a plague tale"))
                return ("Asobo Studio", "Focus Entertainment");
            if (t.Contains("lies of p"))
                return ("Round8 Studio", "Neowiz");
            if (t.Contains("black myth: wukong"))
                return ("Game Science", "Game Science");
            if (t.Contains("stellar blade"))
                return ("Shift Up", "PlayStation Studios");
            if (t.Contains("va-11 hall-a"))
                return ("Sukeban Games", "Ysbryd Games");
            if (t.Contains("cyberpunk 2077") || t.Contains("witcher"))
                return ("CD Projekt Red", "CD Projekt Red");
            if (t.Contains("baldur's gate 3") || t.Contains("baldur's gate iii") || t.Contains("divinity: original sin"))
                return ("Larian Studios", "Larian Studios");
            if (t.Contains("hollow knight"))
                return ("Team Cherry", "Team Cherry");
            if (t.Contains("hades"))
                return ("Supergiant Games", "Supergiant Games");
            if (t.Contains("celeste"))
                return ("Maddy Makes Games", "Maddy Makes Games");
            if (t.Contains("cuphead"))
                return ("Studio MDHR", "Studio MDHR");
            if (t.Contains("disco elysium"))
                return ("ZA/UM", "ZA/UM");
            if (t.Contains("stardew valley"))
                return ("ConcernedApe", "ConcernedApe");
            if (t.Contains("subnautica"))
                return ("Unknown Worlds Entertainment", "Unknown Worlds Entertainment");
            if (t.Contains("dead cells"))
                return ("Motion Twin", "Motion Twin");
            if (t.Contains("slay the spire"))
                return ("Mega Crit Games", "Mega Crit Games");
            if (t.Contains("hitman") && (t.Contains("world of assassination") || t.Contains("3") || t.Contains("iii") || t.Contains("blood money") || t.Contains("absolution")))
                return ("IO Interactive", "IO Interactive");

            // Fallback padrão
            var dev = !string.IsNullOrWhiteSpace(devAtual) ? NormalizadorEmpresaHelper.NormalizarNomeEmpresa(devAtual) : "Desenvolvedor Independente";
            var pub = !string.IsNullOrWhiteSpace(pubAtual) ? NormalizadorEmpresaHelper.NormalizarNomeEmpresa(pubAtual) : dev;
            return (dev, pub);
        }
    }
}
