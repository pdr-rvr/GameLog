using GameLog_Backend.Database;
using GameLog_Backend.Entities;

public class EmpresaSeeder
{
    private readonly GameLogContext _context;

    public EmpresaSeeder(GameLogContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (!_context.Empresa.Any())
        {
            var empresas = new[]
            {
                new Empresa { NomeEmpresa = "Nintendo", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Sony Interactive Entertainment", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Microsoft (Xbox Game Studios)", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Sega", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Capcom", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Konami", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Ubisoft", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Square Enix", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Electronic Arts (EA)", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Bandai Namco", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Rockstar Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Valve", EstaAtivo = true },
                new Empresa { NomeEmpresa = "CD Projekt Red", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Bethesda Softworks", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Riot Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Epic Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Blizzard Entertainment", EstaAtivo = true },
                new Empresa { NomeEmpresa = "FromSoftware", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Insomniac Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Devolver Digital", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Naughty Dog", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Santa Monica Studio", EstaAtivo = true },
                new Empresa { NomeEmpresa = "BioWare", EstaAtivo = true },
                new Empresa { NomeEmpresa = "2K Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Paradox Interactive", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Warner Bros. Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Behaviour Interactive", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Mojang Studios", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Hello Games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "Techland", EstaAtivo = true }
            };

            _context.Empresa.AddRange(empresas);
            _context.SaveChanges();
        }
    }
}