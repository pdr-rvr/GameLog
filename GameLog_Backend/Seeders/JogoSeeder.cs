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

            var jogos = new[]
            {
                // nintendo
                new Jogo {
                    Titulo = "The Legend of Zelda: Breath of the Wild",
                    Descricao = "Acompanha Link em uma aventura de mundo aberto para derrotar Calamity Ganon e restaurar Hyrule.",
                    DataLancamento = new DateOnly(2017, 3, 3),
                    ClassificacaoIndicativa = 10,
                    Empresa = empresas.First(e => e.NomeEmpresa == "nintendo"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Super Mario Odyssey",
                    Descricao = "Mario viaja por diversos reinos para salvar a Princesa Peach de Bowser, utilizando seu chapéu Cappy.",
                    DataLancamento = new DateOnly(2017, 10, 27),
                    ClassificacaoIndicativa = 0,
                    Empresa = empresas.First(e => e.NomeEmpresa == "nintendo"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Animal Crossing: New Horizons",
                    Descricao = "Crie e personalize sua própria ilha deserta, interagindo com moradores e desenvolvendo a comunidade.",
                    DataLancamento = new DateOnly(2020, 3, 20),
                    ClassificacaoIndicativa = 0,
                    Empresa = empresas.First(e => e.NomeEmpresa == "nintendo"),
                    EstaAtivo = true
                },

                // sony
                new Jogo {
                    Titulo = "God of War",
                    Descricao = "Kratos e seu filho Atreus embarcam em uma jornada pela mitologia nórdica, enfrentando deuses e monstros.",
                    DataLancamento = new DateOnly(2018, 4, 20),
                    ClassificacaoIndicativa = 18,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sony"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "The Last of Us Part II",
                    Descricao = "Cinco anos após sua perigosa jornada, Ellie e Joel vivem em Jackson. Um evento violento interrompe a paz.",
                    DataLancamento = new DateOnly(2020, 6, 19),
                    ClassificacaoIndicativa = 18,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sony"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Horizon Zero Dawn",
                    Descricao = "Aloy explora um mundo dominado por máquinas, descobrindo segredos do passado e enfrentando ameaças.",
                    DataLancamento = new DateOnly(2017, 2, 28),
                    ClassificacaoIndicativa = 14,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sony"),
                    EstaAtivo = true
                },

                // microsoft
                new Jogo {
                    Titulo = "Halo Infinite",
                    Descricao = "Master Chief retorna para enfrentar o Banished em um vasto mundo aberto no anel Halo Zeta.",
                    DataLancamento = new DateOnly(2021, 12, 8),
                    ClassificacaoIndicativa = 14,
                    Empresa = empresas.First(e => e.NomeEmpresa == "microsoft"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Forza Horizon 5",
                    Descricao = "Corridas em mundo aberto ambientadas no México, com uma variedade de carros e eventos.",
                    DataLancamento = new DateOnly(2021, 11, 9),
                    ClassificacaoIndicativa = 0,
                    Empresa = empresas.First(e => e.NomeEmpresa == "microsoft"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Gears 5",
                    Descricao = "Kait Diaz desvenda a origem dos Locust, enfrentando desafios e revelações pessoais.",
                    DataLancamento = new DateOnly(2019, 9, 10),
                    ClassificacaoIndicativa = 18,
                    Empresa = empresas.First(e => e.NomeEmpresa == "microsoft"),
                    EstaAtivo = true
                },

                // sega
                new Jogo {
                    Titulo = "Sonic the Hedgehog",
                    Descricao = "O ouriço azul veloz enfrenta o Dr. Robotnik para salvar seus amigos e coletar Chaos Emeralds.",
                    DataLancamento = new DateOnly(1991, 6, 23),
                    ClassificacaoIndicativa = 0,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sega"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Yakuza: Like a Dragon",
                    Descricao = "Ichiban Kasuga explora Yokohama em uma aventura de RPG, enfrentando desafios e formando alianças.",
                    DataLancamento = new DateOnly(2020, 11, 10),
                    ClassificacaoIndicativa = 18,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sega"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Total War: Warhammer",
                    Descricao = "Estratégia em tempo real ambientada no universo de Warhammer, com batalhas épicas entre facções.",
                    DataLancamento = new DateOnly(2016, 5, 24),
                    ClassificacaoIndicativa = 14,
                    Empresa = empresas.First(e => e.NomeEmpresa == "sega"),
                    EstaAtivo = true
                },
                new Jogo {
                    Titulo = "Cyberpunk 2077",
                    Descricao = "RPG de mundo aberto ambientado em uma metrópole futurista dominada por tecnologia e crime.",
                    DataLancamento = new DateOnly(2020, 12, 10),
                    ClassificacaoIndicativa = 18,
                    Empresa = empresas.First(e => e.NomeEmpresa == "cd projekt red"),
                    EstaAtivo = true
                }
            };

            _context.Jogos.AddRange(jogos);
            _context.SaveChanges();
        }
    }
}