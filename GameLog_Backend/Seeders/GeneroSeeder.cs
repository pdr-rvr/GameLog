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
                new Genero { TituloGenero = "acao", EstaAtivo = true },
                new Genero { TituloGenero = "aventura", EstaAtivo = true },
                new Genero { TituloGenero = "rpg", EstaAtivo = true },
                new Genero { TituloGenero = "estrategia", EstaAtivo = true },
                new Genero { TituloGenero = "fps", EstaAtivo = true },
                new Genero { TituloGenero = "corrida", EstaAtivo = true },
                new Genero { TituloGenero = "esporte", EstaAtivo = true },
                new Genero { TituloGenero = "simulacao", EstaAtivo = true },
                new Genero { TituloGenero = "plataforma", EstaAtivo = true },
                new Genero { TituloGenero = "luta", EstaAtivo = true },
                new Genero { TituloGenero = "terror", EstaAtivo = true },
                new Genero { TituloGenero = "puzzle", EstaAtivo = true },
                new Genero { TituloGenero = "mundo aberto", EstaAtivo = true },
                new Genero { TituloGenero = "battle royale", EstaAtivo = true },
                new Genero { TituloGenero = "hack and slash", EstaAtivo = true },
                new Genero { TituloGenero = "furtivo", EstaAtivo = true },
                new Genero { TituloGenero = "roguelike", EstaAtivo = true },
                new Genero { TituloGenero = "visual novel", EstaAtivo = true },
                new Genero { TituloGenero = "moba", EstaAtivo = true },
                new Genero { TituloGenero = "sobrevivencia", EstaAtivo = true }
            };

            _context.Generos.AddRange(generos);
            _context.SaveChanges();
        }
    }
}