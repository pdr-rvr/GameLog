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
            { "Santa Monica Studio", "Santa Monica Studio" },
            { "Naughty Dog", "Naughty Dog" },
            { "Insomniac Games", "Insomniac Games" },
            { "Guerrilla Games", "Guerrilla Games" },
            { "Sucker Punch Productions", "Sucker Punch Productions" },
            { "Polyphony Digital", "Polyphony Digital" },
            { "Bend Studio", "Bend Studio" },
            { "Housemarque", "Housemarque" },

            // Xbox / Microsoft
            { "Microsoft Studios", "Xbox Game Studios" },
            { "Xbox Game Studios", "Xbox Game Studios" },
            { "Microsoft Corporation", "Xbox Game Studios" },
            { "343 Industries", "343 Industries" },
            { "Halo Studios", "343 Industries" },
            { "The Coalition", "The Coalition" },
            { "Turn 10 Studios", "Turn 10 Studios" },
            { "Playground Games", "Playground Games" },
            { "Rare Ltd.", "Rare" },
            { "Rare", "Rare" },
            { "Obsidian Entertainment", "Obsidian Entertainment" },
            { "Ninja Theory", "Ninja Theory" },
            { "InXile Entertainment", "inXile Entertainment" },
            { "Double Fine Productions", "Double Fine Productions" },
            { "Undead Labs", "Undead Labs" },

            // Nintendo
            { "Nintendo EPD", "Nintendo EPD" },
            { "Nintendo of America", "Nintendo" },
            { "Nintendo of Europe", "Nintendo" },
            { "Monolith Soft", "Monolith Soft" },
            { "Game Freak", "Game Freak" },
            { "Creatures Inc.", "Creatures Inc." },
            { "HAL Laboratory", "HAL Laboratory" },
            { "Intelligent Systems", "Intelligent Systems" },
            { "Retro Studios", "Retro Studios" },
            { "Next Level Games", "Next Level Games" },

            // Electronic Arts
            { "Electronic Arts", "Electronic Arts" },
            { "EA Games", "Electronic Arts" },
            { "EA Sports", "Electronic Arts" },
            { "BioWare", "BioWare" },
            { "Respawn Entertainment", "Respawn Entertainment" },
            { "DICE", "DICE" },
            { "EA DICE", "DICE" },
            { "Criterion Games", "Criterion Games" },
            { "Maxis", "Maxis" },
            { "Visceral Games", "Visceral Games" },
            { "Hazelight Studios", "Hazelight Studios" },

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
            { "Crystal Dynamics", "Crystal Dynamics" },
            { "Eidos Montreal", "Eidos Montreal" },

            // FromSoftware
            { "FromSoftware, Inc.", "FromSoftware" },
            { "FromSoftware", "FromSoftware" },
            { "From Software", "FromSoftware" },

            // Rockstar
            { "Rockstar Games", "Rockstar Games" },
            { "Rockstar North", "Rockstar North" },
            { "Rockstar San Diego", "Rockstar San Diego" },
            { "Rockstar Studios", "Rockstar Studios" },

            // Ubisoft
            { "Ubisoft", "Ubisoft" },
            { "Ubisoft Entertainment", "Ubisoft" },
            { "Ubisoft Montreal", "Ubisoft Montreal" },
            { "Ubisoft Quebec", "Ubisoft Quebec" },

            // Bethesda & Zenimax
            { "Bethesda Softworks", "Bethesda Softworks" },
            { "Bethesda Game Studios", "Bethesda Game Studios" },
            { "id Software", "id Software" },
            { "Arkane Studios", "Arkane Studios" },
            { "MachineGames", "MachineGames" },
            { "Tango Gameworks", "Tango Gameworks" },

            // Bandai Namco
            { "Bandai Namco Entertainment", "Bandai Namco Entertainment" },
            { "Namco Bandai Games", "Bandai Namco Entertainment" },
            { "Bandai", "Bandai Namco Entertainment" },
            { "Namco", "Bandai Namco Entertainment" },

            // SEGA / Atlus
            { "SEGA", "SEGA" },
            { "Atlus", "Atlus" },
            { "Ryu Ga Gotoku Studio", "Ryu Ga Gotoku Studio" },
            { "Creative Assembly", "Creative Assembly" },
            { "Sonic Team", "Sonic Team" },

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
            { "Infinity Ward", "Infinity Ward" },
            { "Treyarch", "Treyarch" },
            { "Sledgehammer Games", "Sledgehammer Games" },

            // Warner Bros
            { "Warner Bros. Games", "Warner Bros. Games" },
            { "Warner Bros. Interactive Entertainment", "Warner Bros. Games" },
            { "Rocksteady Studios", "Rocksteady Studios" },
            { "NetherRealm Studios", "NetherRealm Studios" },
            { "Monolith Productions", "Monolith Productions" },
            { "Avalanche Software", "Avalanche Software" },
            { "WB Games Montreal", "WB Games Montreal" },

            // 2K / Take-Two
            { "2K", "2K Games" },
            { "2K Games", "2K Games" },
            { "Firaxis Games", "Firaxis Games" },
            { "Hangar 13", "Hangar 13" },
            { "Gearbox Software", "Gearbox Software" },
            { "Irrational Games", "Irrational Games" }
        };

        public static string NormalizarNomeEmpresa(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return "Independente";
            var trim = nome.Trim();

            if (NormalizedCompanies.TryGetValue(trim, out var canonica))
            {
                return canonica;
            }

            // Normalização por aproximação precisa (sem sobrepor desenvolvedoras distintas)
            var lower = trim.ToLowerInvariant();
            if (lower == "sony" || lower.Contains("sony interactive") || lower.Contains("playstation studios") || lower.Contains("sony computer"))
                return "PlayStation Studios";
            if (lower == "microsoft" || lower.Contains("microsoft studios") || lower.Contains("xbox game studios"))
                return "Xbox Game Studios";
            if (lower == "nintendo" || lower.Contains("nintendo of"))
                return "Nintendo";
            if (lower.StartsWith("electronic arts") || lower.Contains("ea games") || lower.Contains("ea sports"))
                return "Electronic Arts";
            if (lower.StartsWith("rockstar north"))
                return "Rockstar North";
            if (lower.StartsWith("rockstar san diego"))
                return "Rockstar San Diego";
            if (lower.StartsWith("rockstar studios"))
                return "Rockstar Studios";
            if (lower.StartsWith("rockstar games") || lower == "rockstar" || lower.StartsWith("rockstar inc"))
                return "Rockstar Games";
            if (lower == "cd projekt" || lower.Contains("cd projekt red"))
                return "CD Projekt Red";
            if (lower == "capcom" || lower.Contains("capcom co") || lower.StartsWith("capcom "))
                return "Capcom";
            if (lower.Contains("square enix") || lower.Contains("squaresoft") || lower.Contains("square co"))
                return "Square Enix";
            if (lower.Contains("fromsoftware") || lower.Contains("from software"))
                return "FromSoftware";
            if (lower.Contains("ubisoft"))
                return "Ubisoft";
            if (lower.Contains("bethesda softworks"))
                return "Bethesda Softworks";
            if (lower.Contains("bandai namco") || lower.Contains("namco bandai"))
                return "Bandai Namco Entertainment";
            if (lower == "sega" || lower.Contains("sega of") || lower.Contains("sega games"))
                return "SEGA";
            if (lower.Contains("konami"))
                return "Konami";
            if (lower.Contains("valve"))
                return "Valve";
            if (lower.Contains("blizzard"))
                return "Blizzard Entertainment";
            if (lower == "activision" || lower.Contains("activision publishing"))
                return "Activision";
            if (lower.Contains("warner bros"))
                return "Warner Bros. Games";
            if (lower == "2k" || lower.Contains("2k games") || lower.Contains("take-two"))
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

            if (t.Contains("vampire: the masquerade") || t.Contains("crusader kings") || t.Contains("europa universalis") ||
                t.Contains("hearts of iron") || t.Contains("stellaris") || t.Contains("cities: skylines") ||
                t.Contains("prison architect") || t.Contains("age of wonders") || t.Contains("victoria"))
                return "Paradox Interactive";

            if (t.Contains("gothic") || t.Contains("darksiders") || t.Contains("biomutant") ||
                t.Contains("destroy all humans") || t.Contains("titan quest") || t.Contains("spellforce") ||
                t.Contains("elex") || t.Contains("risen") || t.Contains("alone in the dark") ||
                t.Contains("saints row") || t.Contains("metro 2033") || t.Contains("metro last light") ||
                t.Contains("metro exodus") || t.Contains("dead island") || t.Contains("kingdom come"))
                return "THQ Nordic";

            if (t.Contains("cult of the lamb") || t.Contains("hotline miami") || t.Contains("enter the gungeon") ||
                t.Contains("the talos principle") || t.Contains("serious sam") || t.Contains("inscryption") ||
                t.Contains("gris") || t.Contains("katana zero") || t.Contains("loop hero") ||
                t.Contains("death's door") || t.Contains("the messenger") || t.Contains("shadow warrior") ||
                t.Contains("broforce"))
                return "Devolver Digital";

            if (t.Contains("hades") || t.Contains("bastion") || t.Contains("transistor") || t.Contains("pyre"))
                return "Supergiant Games";

            if (t.Contains("a plague tale") || t.Contains("vampyr") || t.Contains("banishers") ||
                t.Contains("snowrunner") || t.Contains("mudrunner") || t.Contains("space marine") ||
                t.Contains("atomic heart") || t.Contains("the surge") || t.Contains("atlas fallen"))
                return "Focus Entertainment";

            if (t.Contains("control") || t.Contains("ghostrunner") || t.Contains("bloodstained") ||
                t.Contains("payday") || t.Contains("assetto corsa"))
                return "505 Games";

            if (t.Contains("overcooked") || t.Contains("blasphemous") || t.Contains("dredge") ||
                t.Contains("hell let loose") || t.Contains("the escapists") || t.Contains("worms") ||
                t.Contains("trepang2") || t.Contains("my time at"))
                return "Team17";

            if (t.Contains("outer wilds") || t.Contains("stray") || t.Contains("edith finch") ||
                t.Contains("neon white") || t.Contains("cocoon") || t.Contains("solar ash") ||
                t.Contains("hyper light drifter"))
                return "Annapurna Interactive";

            if (t.Contains("escape from tarkov"))
                return "Battlestate Games";

            if (t.Contains("cronos: the new dawn") || t.Contains("the medium") || t.Contains("layers of fear") || t.Contains("observer"))
                return "Bloober Team";

            if (t.Contains("star citizen") || t.Contains("squadron 42"))
                return "Cloud Imperium Games";

            if (t.Contains("hitman") || t.Contains("007 first light"))
                return "IO Interactive";

            if (t.Contains("hollow knight") || t.Contains("silksong"))
                return "Team Cherry";

            if (t.Contains("stardew valley") || t.Contains("haunted chocolatier"))
                return "ConcernedApe";

            if (t.Contains("terraria"))
                return "Re-Logic";

            if (t.Contains("minecraft"))
                return "Mojang Studios";

            if (t.Contains("rust") || t.Contains("garry's mod"))
                return "Facepunch Studios";

            if (t.Contains("among us"))
                return "Innersloth";

            if (t.Contains("slay the spire"))
                return "Mega Crit";

            if (t.Contains("valheim") || t.Contains("satisfactory") || t.Contains("deep rock galactic") || t.Contains("goat simulator"))
                return "Coffee Stain Publishing";

            if (t.Contains("subnautica"))
                return "Unknown Worlds Entertainment";

            if (t.Contains("the forest") || t.Contains("sons of the forest"))
                return "Endnight Games";

            if (t.Contains("rimworld"))
                return "Ludeon Studios";

            if (t.Contains("factorio"))
                return "Wube Software";

            if (t.Contains("no man's sky"))
                return "Hello Games";

            if (t.Contains("sifu") || t.Contains("pacific drive") || t.Contains("scorn") || t.Contains("cat quest"))
                return "Kepler Interactive";

            if (t.Contains("sea of stars"))
                return "Sabotage Studio";

            if (t.Contains("balatro"))
                return "LocalThunk";

            if (t.Contains("lethal company"))
                return "Zeekerss";

            if (t.Contains("dead cells") || t.Contains("windblown"))
                return "Motion Twin";

            if (t.Contains("darkest dungeon"))
                return "Red Hook Studios";

            if (t.Contains("binding of isaac") || t.Contains("super meat boy") || t.Contains("mewgenics"))
                return "Edmund McMillen";

            return null;
        }

        public static (string Desenvolvedora, string? Publicadora) ResolverParDesenvolvedoraPublicadora(string titulo, string? devAtual, string? pubAtual)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return (devAtual ?? "Independente", pubAtual);
            var t = titulo.ToLowerInvariant();

            // FromSoftware
            if (t.Contains("elden ring") || t.Contains("dark souls") || t.Contains("armored core"))
                return ("FromSoftware", "Bandai Namco Entertainment");
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
                return ("Irrational Games", "2K Games");
            if (t.Contains("borderlands") || t.Contains("tiny tina") || t.Contains("battleborn"))
                return ("Gearbox Software", "2K Games");
            if (t.Contains("civilization") || t.Contains("xcom") || t.Contains("midnight suns"))
                return ("Firaxis Games", "2K Games");
            if (t.Contains("mafia"))
                return ("Hangar 13", "2K Games");
            if (t.Contains("nba 2k") || t.Contains("wwe 2k"))
                return ("Visual Concepts", "2K Games");

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
                return ("Bandai Namco Entertainment", "Nintendo");
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
                return ("Bandai Namco Entertainment", "Bandai Namco Entertainment");

            // Konami
            if (t.Contains("metal gear") || t.Contains("silent hill") || t.Contains("castlevania") || t.Contains("pes ") || t.Contains("efootball"))
                return ("Konami", "Konami");

            // Blizzard
            if (t.Contains("warcraft") || t.Contains("diablo") || t.Contains("starcraft") || t.Contains("overwatch"))
                return ("Blizzard Entertainment", "Activision");

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
            var dev = !string.IsNullOrWhiteSpace(devAtual) ? NormalizarNomeEmpresa(devAtual) : "Independente";
            var pub = !string.IsNullOrWhiteSpace(pubAtual) ? NormalizarNomeEmpresa(pubAtual) : dev;
            return (dev, pub);
        }
    }
}
