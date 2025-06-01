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

                // Nintendo
                if (tituloJogo.Contains("zelda"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                    jogo.Generos.Add(generosDict["rpg"]);
                    jogo.Generos.Add(generosDict["sandbox"]);
                }
                else if (tituloJogo.Contains("mario odyssey"))
                {
                    jogo.Generos.Add(generosDict["plataforma"]);
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                }
                else if (tituloJogo.Contains("animal crossing"))
                {
                    jogo.Generos.Add(generosDict["simulação"]);
                    jogo.Generos.Add(generosDict["sandbox"]);
                }
                else if (tituloJogo.Contains("metroid"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                    jogo.Generos.Add(generosDict["metroidvania"]);
                }
                else if (tituloJogo.Contains("splatoon"))
                {
                    jogo.Generos.Add(generosDict["tiro"]);
                    jogo.Generos.Add(generosDict["ação"]);
                }

                // Sony
                else if (tituloJogo.Contains("god of war"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                    jogo.Generos.Add(generosDict["rpg"]);
                }
                else if (tituloJogo.Contains("last of us"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                    jogo.Generos.Add(generosDict["horror"]);
                }
                else if (tituloJogo.Contains("horizon"))
                {
                    jogo.Generos.Add(generosDict["rpg"]);
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                }
                else if (tituloJogo.Contains("ghost of tsushima"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                    jogo.Generos.Add(generosDict["rpg"]);
                }
                else if (tituloJogo.Contains("spider-man"))
                {
                    jogo.Generos.Add(generosDict["ação-aventura"]);
                }

                // Microsoft
                else if (tituloJogo.Contains("halo"))
                {
                    jogo.Generos.Add(generosDict["fps"]);
                    jogo.Generos.Add(generosDict["ação"]);
                }
                else if (tituloJogo.Contains("forza"))
                {
                    jogo.Generos.Add(generosDict["corrida"]);
                    jogo.Generos.Add(generosDict["simulação"]);
                }
                else if (tituloJogo.Contains("gears"))
                {
                    jogo.Generos.Add(generosDict["tiro"]);
                    jogo.Generos.Add(generosDict["ação"]);
                }
                else if (tituloJogo.Contains("sea of thieves"))
                {
                    jogo.Generos.Add(generosDict["aventura"]);
                    jogo.Generos.Add(generosDict["ação"]);
                    jogo.Generos.Add(generosDict["sandbox"]);
                }
                else if (tituloJogo.Contains("age of empires"))
                {
                    jogo.Generos.Add(generosDict["estratégia"]);
                }

                else if (tituloJogo.Contains("fifa") || tituloJogo.Contains("football"))
                {
                    jogo.Generos.Add(generosDict["esportes"]);
                }
                else if (tituloJogo.Contains("street fighter") || tituloJogo.Contains("tekken"))
                {
                    jogo.Generos.Add(generosDict["luta"]);
                }
                else if (tituloJogo.Contains("racing") || tituloJogo.Contains("corrida"))
                {
                    jogo.Generos.Add(generosDict["corrida"]);
                }
                else
                {
                    jogo.Generos.Add(generosDict["ação"]);
                }
            }

            _context.SaveChanges();
        }
    }
}