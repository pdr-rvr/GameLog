using GameLog_Backend.Database;
using GameLog_Backend.Entities;

public class GeneroSeeder
{
    private readonly GameLogContext _context;

    public GeneroSeeder(GameLogContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (!_context.Generos.Any())
        {
            var generos = new[]
            {
                new Genero { TituloGenero = "Ação", EstaAtivo = true },
                new Genero { TituloGenero = "Ação-Aventura", EstaAtivo = true },
                new Genero { TituloGenero = "Aventura", EstaAtivo = true },
                new Genero { TituloGenero = "RPG", EstaAtivo = true },
                new Genero { TituloGenero = "FPS", EstaAtivo = true },
                new Genero { TituloGenero = "Tiro", EstaAtivo = true },
                new Genero { TituloGenero = "Estratégia", EstaAtivo = true },
                new Genero { TituloGenero = "Esportes", EstaAtivo = true },
                new Genero { TituloGenero = "Corrida", EstaAtivo = true },
                new Genero { TituloGenero = "Luta", EstaAtivo = true },
                new Genero { TituloGenero = "Plataforma", EstaAtivo = true },
                new Genero { TituloGenero = "Puzzle", EstaAtivo = true },
                new Genero { TituloGenero = "Horror", EstaAtivo = true },
                new Genero { TituloGenero = "Survival", EstaAtivo = true },
                new Genero { TituloGenero = "Battle Royale", EstaAtivo = true },
                new Genero { TituloGenero = "MOBA", EstaAtivo = true },
                new Genero { TituloGenero = "MMORPG", EstaAtivo = true },
                new Genero { TituloGenero = "Simulação", EstaAtivo = true },
                new Genero { TituloGenero = "Musical", EstaAtivo = true },
                new Genero { TituloGenero = "Sandbox", EstaAtivo = true },
                new Genero { TituloGenero = "Metroidvania", EstaAtivo = true },
                new Genero { TituloGenero = "Roguelike", EstaAtivo = true },
                new Genero { TituloGenero = "Point-and-Click", EstaAtivo = true },
                new Genero { TituloGenero = "Visual Novel", EstaAtivo = true }
            };

            _context.Generos.AddRange(generos);
            _context.SaveChanges();
        }
    }
}