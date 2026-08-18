// JogoGeneroSeeder.cs
using GameLog_Backend.Database;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; 

public class JogoGeneroSeeder
{
    private readonly GameLogContext _context;

    public JogoGeneroSeeder(GameLogContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (_context.Jogos.Include(j => j.Generos).Any(j => j.Generos.Any()))
        {
            Console.WriteLine("Associações Jogo-Gênero já existem. Pulando JogoGeneroSeeder.");
            return;
        }

        var jogos = _context.Jogos.ToList(); 
        var generos = _context.Generos.ToList();

        if (!jogos.Any() || !generos.Any())
            throw new Exception("Execute primeiro os seeders de Jogo e Genero.");

        var generosDict = generos.ToDictionary(g => g.TituloGenero.ToLower(), g => g);

        var jogoGenerosMap = new Dictionary<string, List<string>>
        {
            // Nintendo
            {"the legend of zelda: breath of the wild", new List<string> {"ação-aventura", "rpg", "mundo aberto", "sandbox"}},
            {"super mario odyssey", new List<string> {"plataforma", "ação-aventura"}},
            {"animal crossing: new horizons", new List<string> {"simulação", "sandbox"}},
            {"metroid dread", new List<string> {"ação-aventura", "metroidvania"}},
            {"splatoon 3", new List<string> {"tiro", "ação", "multiplayer"}},

            // Sony
            {"god of war (2018)", new List<string> {"ação-aventura", "rpg", "mitologia"}},
            {"the last of us part ii", new List<string> {"ação-aventura", "horror", "survival", "post-apocalíptico"}},
            {"horizon zero dawn", new List<string> {"rpg", "ação-aventura", "mundo aberto", "ficção científica"}},
            {"ghost of tsushima", new List<string> {"ação-aventura", "rpg", "stealth", "feudal"}},
            {"spider-man: miles morales", new List<string> {"ação-aventura", "super-herói"}},

            // Microsoft
            {"halo infinite", new List<string> {"fps", "ação", "ficção científica", "multiplayer"}},
            {"forza horizon 5", new List<string> {"corrida", "simulação", "mundo aberto"}},
            {"gears 5", new List<string> {"tiro", "ação", "sci-fi"}}, // Usei sci-fi se existir, senão ficção científica
            {"sea of thieves", new List<string> {"aventura", "ação", "sandbox", "multiplayer"}},
            {"age of empires iv", new List<string> {"estratégia", "história"}},

            // Sega
            {"sonic frontiers", new List<string> {"ação", "plataforma", "aventura", "mundo aberto"}},
            {"yakuza: like a dragon", new List<string> {"rpg", "ação", "drama"}},
            {"total war: warhammer iii", new List<string> {"estratégia", "fantasia"}},
            {"persona 5 royal", new List<string> {"rpg", "j-rpg", "simulação de vida"}},
            {"football manager 2023", new List<string> {"esportes", "simulação", "gerenciamento"}},

            // Capcom
            {"resident evil village", new List<string> {"survival horror", "ação", "horror"}},
            {"monster hunter rise", new List<string> {"ação", "rpg", "coop"}},
            {"street fighter 6", new List<string> {"luta", "arcade"}},
            {"devil may cry 5", new List<string> {"ação", "hack and slash"}},
            {"mega man 11", new List<string> {"plataforma", "ação"}},

            // Konami
            {"efootball 2023", new List<string> {"esportes", "simulação"}},
            {"metal gear solid v", new List<string> {"stealth", "ação", "ficção científica"}},
            {"castlevania: symphony of the night", new List<string> {"metroidvania", "rpg", "plataforma", "fantasia"}},
            {"silent hill 2", new List<string> {"horror", "survival", "psicológico"}},
            {"pro evolution soccer 2019", new List<string> {"esportes", "simulação"}},

            // Ubisoft
            {"assassin's creed valhalla", new List<string> {"rpg", "ação-aventura", "mundo aberto", "história", "viking"}},
            {"far cry 6", new List<string> {"fps", "ação", "mundo aberto"}},
            {"rainbow six siege", new List<string> {"fps", "tático", "multiplayer"}},
            {"watch dogs: legion", new List<string> {"ação-aventura", "mundo aberto", "ficção científica", "stealth"}},
            {"the division 2", new List<string> {"rpg", "tiro", "coop", "multiplayer"}},

            // Square Enix
            {"final fantasy vii remake", new List<string> {"rpg", "ação", "fantasia", "j-rpg"}},
            {"kingdom hearts iii", new List<string> {"ação-rpg", "fantasia"}},
            {"nier: automata", new List<string> {"ação-rpg", "ficção científica", "filosófico"}}, // "filosófico" é mais como tag, verifique se seu GeneroSeeder tem
            {"marvel's guardians of the galaxy", new List<string> {"ação-aventura", "super-herói", "ficção científica"}},
            {"dragon quest xi", new List<string> {"rpg", "j-rpg", "fantasia"}},

            // CD Projekt Red
            {"cyberpunk 2077", new List<string> {"rpg", "ação", "mundo aberto", "ficção científica", "cyberpunk"}}, // Gêneros específicos para Cyberpunk 2077
            {"the witcher 3: wild hunt", new List<string> {"rpg", "ação-aventura", "mundo aberto", "fantasia", "medieval"}},

            // Outras empresas
            {"league of legends", new List<string> {"moba", "estratégia", "multiplayer"}},
            {"valorant", new List<string> {"fps", "tático", "multiplayer"}},
            {"fortnite", new List<string> {"battle royale", "tiro", "construção", "multiplayer"}},
            {"diablo iv", new List<string> {"rpg", "ação", "hack and slash", "dark fantasy"}},
            {"overwatch 2", new List<string> {"fps", "hero shooter", "multiplayer"}},
            {"elden ring", new List<string> {"rpg", "ação-aventura", "mundo aberto", "dark fantasy", "souls-like", "medieval"}},
            {"dark souls iii", new List<string> {"rpg", "ação", "dark fantasy", "souls-like", "medieval"}},
            {"minecraft", new List<string> {"sandbox", "construção", "aventura", "exploração", "sobrevivência"}},
            {"no man's sky", new List<string> {"exploração", "sandbox", "ficção científica", "survival"}},
            {"dying light 2", new List<string> {"survival horror", "ação", "parkour", "zumbi", "mundo aberto"}}
        };

        foreach (var jogo in jogos)
        {
            var tituloNormalizado = jogo.Titulo.ToLower();

            if (jogoGenerosMap.TryGetValue(tituloNormalizado, out var generosDoJogoTitulos))
            {
                foreach (var generoTitulo in generosDoJogoTitulos)
                {
                    if (generosDict.TryGetValue(generoTitulo.ToLower(), out var generoEntity))
                    {
                        if (!jogo.Generos.Any(g => g.Id == generoEntity.Id)) 
                        {
                            jogo.Generos.Add(generoEntity);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Aviso: Gênero '{generoTitulo}' não encontrado no GeneroSeeder para o jogo '{jogo.Titulo}'.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Aviso: Jogo '{jogo.Titulo}' não possui mapeamento de gêneros explícito no seeder.");
            }
        }

        _context.SaveChanges();
        Console.WriteLine("Associações Jogo-Gênero criadas/atualizadas com sucesso.");
    }
}