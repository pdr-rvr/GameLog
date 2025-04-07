using GameLog_Backend.Database;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;

public class JogoGeneroSeeder
{
    private readonly GameLogContext _context;

    public JogoGeneroSeeder(GameLogContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (!_context.Jogos.Include(j => j.Generos).Any(j => j.Generos.Any()))
        {
            var jogos = _context.Jogos.Include(j => j.Generos).ToList();
            var generos = _context.Generos.ToList();

            if (!jogos.Any() || !generos.Any())
                throw new Exception("Execute primeiro os seeders de Jogo e Genero");

            var generosDict = generos.ToDictionary(g => g.TituloGenero.ToLower());

            foreach (var jogo in jogos)
            {
                var tituloJogo = jogo.Titulo.ToLower();

                if (tituloJogo.Contains("zelda"))
                {
                    jogo.Generos.Add(generosDict["aventura"]);
                    jogo.Generos.Add(generosDict["acao"]);
                    jogo.Generos.Add(generosDict["rpg"]);
                }
                else if (tituloJogo.Contains("god of war"))
                {
                    jogo.Generos.Add(generosDict["acao"]);
                    jogo.Generos.Add(generosDict["hack and slash"]);
                }
                else if (tituloJogo.Contains("halo"))
                {
                    jogo.Generos.Add(generosDict["fps"]);
                    jogo.Generos.Add(generosDict["acao"]);
                }
                else if (tituloJogo.Contains("forza"))
                {
                    jogo.Generos.Add(generosDict["corrida"]);
                }
                else if (tituloJogo.Contains("animal crossing"))
                {
                    jogo.Generos.Add(generosDict["simulacao"]);
                }
            }

            _context.SaveChanges();
        }
    }
}