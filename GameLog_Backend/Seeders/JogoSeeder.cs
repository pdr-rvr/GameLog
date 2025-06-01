using GameLog_Backend.Database;
using GameLog_Backend.Entities;

public class JogoSeeder
{
    private readonly GameLogContext _context;

    public JogoSeeder(GameLogContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (!_context.Jogos.Any())
        {
            var empresas = _context.Empresa.ToList();

            var jogos = new List<Jogo>
            {
                // Nintendo
                CriarJogo("The Legend of Zelda: Breath of the Wild", "Aventura épica em mundo aberto", new DateOnly(2017, 3, 3), 10, "Nintendo"),
                CriarJogo("Super Mario Odyssey", "Aventura 3D com mecânicas de chapéu", new DateOnly(2017, 10, 27), 0, "Nintendo"),
                CriarJogo("Animal Crossing: New Horizons", "Simulação de vida em ilha", new DateOnly(2020, 3, 20), 0, "Nintendo"),
                CriarJogo("Metroid Dread", "Ação-aventura em side-scrolling", new DateOnly(2021, 10, 8), 12, "Nintendo"),
                CriarJogo("Splatoon 3", "Tiro em terceira pessoa com tinta", new DateOnly(2022, 9, 9), 10, "Nintendo"),

                // Sony
                CriarJogo("God of War (2018)", "Ação-aventura na mitologia nórdica", new DateOnly(2018, 4, 20), 18, "Sony Interactive Entertainment"),
                CriarJogo("The Last of Us Part II", "Narrativa intensa em mundo pós-apocalíptico", new DateOnly(2020, 6, 19), 18, "Sony Interactive Entertainment"),
                CriarJogo("Horizon Zero Dawn", "RPG de ação em mundo aberto futurista", new DateOnly(2017, 2, 28), 14, "Sony Interactive Entertainment"),
                CriarJogo("Ghost of Tsushima", "Ação em mundo aberto no Japão feudal", new DateOnly(2020, 7, 17), 18, "Sony Interactive Entertainment"),
                CriarJogo("Spider-Man: Miles Morales", "Ação-aventura com o Homem-Aranha", new DateOnly(2020, 11, 12), 16, "Sony Interactive Entertainment"),

                // Microsoft
                CriarJogo("Halo Infinite", "FPS com Master Chief", new DateOnly(2021, 12, 8), 14, "Microsoft (Xbox Game Studios)"),
                CriarJogo("Forza Horizon 5", "Corrida em mundo aberto no México", new DateOnly(2021, 11, 9), 0, "Microsoft (Xbox Game Studios)"),
                CriarJogo("Gears 5", "Tiro em terceira pessoa", new DateOnly(2019, 9, 10), 18, "Microsoft (Xbox Game Studios)"),
                CriarJogo("Sea of Thieves", "Aventura pirata multiplayer", new DateOnly(2018, 3, 20), 12, "Microsoft (Xbox Game Studios)"),
                CriarJogo("Age of Empires IV", "Estratégia em tempo real histórico", new DateOnly(2021, 10, 28), 12, "Microsoft (Xbox Game Studios)"),

                // Sega
                CriarJogo("Sonic Frontiers", "Aventura em mundo aberto com Sonic", new DateOnly(2022, 11, 8), 7, "Sega"),
                CriarJogo("Yakuza: Like a Dragon", "RPG com temática de Yakuza", new DateOnly(2020, 11, 10), 18, "Sega"),
                CriarJogo("Total War: Warhammer III", "Estratégia em grande escala", new DateOnly(2022, 2, 17), 16, "Sega"),
                CriarJogo("Persona 5 Royal", "RPG com elementos de vida escolar", new DateOnly(2020, 3, 31), 16, "Sega"),
                CriarJogo("Football Manager 2023", "Simulador de gerenciamento de futebol", new DateOnly(2022, 11, 8), 3, "Sega"),

                // Capcom
                CriarJogo("Resident Evil Village", "Survival horror em primeira pessoa", new DateOnly(2021, 5, 7), 18, "Capcom"),
                CriarJogo("Monster Hunter Rise", "Caça a monstros em ação cooperativa", new DateOnly(2021, 3, 26), 12, "Capcom"),
                CriarJogo("Street Fighter 6", "Luta competitiva", new DateOnly(2023, 6, 2), 12, "Capcom"),
                CriarJogo("Devil May Cry 5", "Ação estilizada com combos", new DateOnly(2019, 3, 8), 18, "Capcom"),
                CriarJogo("Mega Man 11", "Plataforma clássico com nova mecânica", new DateOnly(2018, 10, 2), 10, "Capcom"),

                // Konami
                CriarJogo("eFootball 2023", "Simulador de futebol", new DateOnly(2022, 9, 30), 3, "Konami"),
                CriarJogo("Metal Gear Solid V", "Ação stealth em mundo aberto", new DateOnly(2015, 9, 1), 18, "Konami"),
                CriarJogo("Castlevania: Symphony of the Night", "Clássico metroidvania", new DateOnly(1997, 3, 20), 12, "Konami"),
                CriarJogo("Silent Hill 2", "Survival horror psicológico", new DateOnly(2001, 9, 24), 18, "Konami"),
                CriarJogo("Pro Evolution Soccer 2019", "Simulador de futebol", new DateOnly(2018, 8, 28), 3, "Konami"),

                // Ubisoft
                CriarJogo("Assassin's Creed Valhalla", "RPG de ação na era viking", new DateOnly(2020, 11, 10), 18, "Ubisoft"),
                CriarJogo("Far Cry 6", "FPS em mundo aberto", new DateOnly(2021, 10, 7), 18, "Ubisoft"),
                CriarJogo("Rainbow Six Siege", "FPS tático multiplayer", new DateOnly(2015, 12, 1), 18, "Ubisoft"),
                CriarJogo("Watch Dogs: Legion", "Ação em mundo aberto futurista", new DateOnly(2020, 10, 29), 18, "Ubisoft"),
                CriarJogo("The Division 2", "RPG de tiro cooperativo", new DateOnly(2019, 3, 15), 18, "Ubisoft"),

                // Square Enix
                CriarJogo("Final Fantasy VII Remake", "RPG de ação com narrativa épica", new DateOnly(2020, 4, 10), 16, "Square Enix"),
                CriarJogo("Kingdom Hearts III", "Ação-RPG com personagens da Disney", new DateOnly(2019, 1, 25), 12, "Square Enix"),
                CriarJogo("NieR: Automata", "Ação-RPG filosófico", new DateOnly(2017, 3, 7), 16, "Square Enix"),
                CriarJogo("Marvel's Guardians of the Galaxy", "Ação-aventura baseado nos quadrinhos", new DateOnly(2021, 10, 26), 16, "Square Enix"),
                CriarJogo("Dragon Quest XI", "RPG tradicional japonês", new DateOnly(2017, 7, 29), 12, "Square Enix"),

                // CD Projekt Red
                CriarJogo("Cyberpunk 2077", "RPG de mundo aberto futurista", new DateOnly(2020, 12, 10), 18, "CD Projekt Red"),
                CriarJogo("The Witcher 3: Wild Hunt", "RPG de mundo aberto fantástico", new DateOnly(2015, 5, 19), 18, "CD Projekt Red"),

                // Outras empresas
                CriarJogo("League of Legends", "MOBA competitivo", new DateOnly(2009, 10, 27), 12, "Riot Games"),
                CriarJogo("Valorant", "FPS tático", new DateOnly(2020, 6, 2), 16, "Riot Games"),
                CriarJogo("Fortnite", "Battle Royale com construção", new DateOnly(2017, 7, 21), 12, "Epic Games"),
                CriarJogo("Diablo IV", "RPG de ação sombrio", new DateOnly(2023, 6, 6), 18, "Blizzard Entertainment"),
                CriarJogo("Overwatch 2", "FPS hero-based", new DateOnly(2022, 10, 4), 12, "Blizzard Entertainment"),
                CriarJogo("Elden Ring", "RPG de ação em mundo aberto", new DateOnly(2022, 2, 25), 16, "FromSoftware"),
                CriarJogo("Dark Souls III", "RPG de ação desafiador", new DateOnly(2016, 3, 24), 16, "FromSoftware"),
                CriarJogo("Minecraft", "Sandbox criativo", new DateOnly(2011, 11, 18), 7, "Mojang Studios"),
                CriarJogo("No Man's Sky", "Exploração espacial", new DateOnly(2016, 8, 9), 7, "Hello Games"),
                CriarJogo("Dying Light 2", "Survival horror com parkour", new DateOnly(2022, 2, 4), 18, "Techland")
            };

            _context.Jogos.AddRange(jogos);
            _context.SaveChanges();
        }
    }

    private Jogo CriarJogo(string titulo, string descricao, DateOnly dataLancamento, int classificacao, string nomeEmpresa)
    {
        return new Jogo
        {
            Titulo = titulo,
            Descricao = descricao,
            DataLancamento = dataLancamento,
            ClassificacaoIndicativa = classificacao,
            Empresa = _context.Empresa.First(e => e.NomeEmpresa == nomeEmpresa),
            EstaAtivo = true
        };
    }
}