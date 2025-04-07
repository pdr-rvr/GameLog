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
                new Empresa { NomeEmpresa = "nintendo", EstaAtivo = true },
                new Empresa { NomeEmpresa = "sony", EstaAtivo = true },
                new Empresa { NomeEmpresa = "microsoft", EstaAtivo = true },
                new Empresa { NomeEmpresa = "sega", EstaAtivo = true },
                new Empresa { NomeEmpresa = "capcom", EstaAtivo = true },
                new Empresa { NomeEmpresa = "konami", EstaAtivo = true },
                new Empresa { NomeEmpresa = "ubisoft", EstaAtivo = true },
                new Empresa { NomeEmpresa = "square enix", EstaAtivo = true },
                new Empresa { NomeEmpresa = "ea games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "bandai namco", EstaAtivo = true },
                new Empresa { NomeEmpresa = "rockstar games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "valve", EstaAtivo = true },
                new Empresa { NomeEmpresa = "cd projekt red", EstaAtivo = true },
                new Empresa { NomeEmpresa = "bethesda", EstaAtivo = true },
                new Empresa { NomeEmpresa = "riot games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "epic games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "blizzard", EstaAtivo = true },
                new Empresa { NomeEmpresa = "fromsoftware", EstaAtivo = true },
                new Empresa { NomeEmpresa = "insomniac games", EstaAtivo = true },
                new Empresa { NomeEmpresa = "devolver digital", EstaAtivo = true }
            };

            _context.Empresa.AddRange(empresas);
            _context.SaveChanges();
        }
    }
}