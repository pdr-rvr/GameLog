using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class JogoServicesTests
    {
        private RawgApiService CreateMockRawgService(GameLog_Backend.Database.GameLogContext context)
        {
            var httpClient = new HttpClient();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "Rawg:ApiKey", "test-key" },
                { "Rawg:BaseUrl", "https://api.rawg.io/api" }
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
            var logger = NullLogger<RawgApiService>.Instance;

            return new RawgApiService(httpClient, context, memoryCache, configuration, logger);
        }

        [Fact]
        public async Task ListarJogos_DeveRetornarJogosAtivosComMediaDeNotas()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var rawgService = CreateMockRawgService(context);
            var service = new JogoServices(context, rawgService);

            var empresa = new Empresa { NomeEmpresa = "Nintendo", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogo = new Jogo
            {
                Titulo = "The Legend of Zelda: Breath of the Wild",
                Descricao = "Open world adventure",
                Imagem = "botw.jpg",
                DataLancamento = new DateOnly(2017, 3, 3),
                Empresa = empresa,
                EstaAtivo = true
            };
            context.Jogos.Add(jogo);

            var usuario = new Usuario { NomeUsuario = "Link", Email = "link@hyrule.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var avaliacao1 = new Avaliacao { Jogo = jogo, Usuario = usuario, Nota = 5, EstaAtivo = true };
            var avaliacao2 = new Avaliacao { Jogo = jogo, Usuario = usuario, Nota = 4, EstaAtivo = true };
            context.Avaliacoes.AddRange(avaliacao1, avaliacao2);
            await context.SaveChangesAsync();

            // Act
            var jogos = (await service.ListarJogos()).ToList();

            // Assert
            jogos.Should().ContainSingle();
            var j = jogos.First();
            j.Titulo.Should().Be("The Legend of Zelda: Breath of the Wild");
            j.NomeEmpresa.Should().Be("Nintendo");
            j.MediaAvaliacoes.Should().Be(4.5);
            j.TotalAvaliacoes.Should().Be(2);
        }

        [Fact]
        public async Task ObterJogoPorId_QuandoJogoInativoOuInexistente_DeveRetornarNull()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var rawgService = CreateMockRawgService(context);
            var service = new JogoServices(context, rawgService);

            // Act
            var jogo = await service.ObterJogoPorId(Guid.NewGuid());

            // Assert
            jogo.Should().BeNull();
        }

        [Fact]
        public async Task ObterJogosPaginadosAsync_ComFiltrosDeNotaEEmpresa_DeveFiltrarCorretamente()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var rawgService = CreateMockRawgService(context);
            var service = new JogoServices(context, rawgService);

            var sony = new Empresa { NomeEmpresa = "Sony Interactive Entertainment", EstaAtivo = true };
            var capcom = new Empresa { NomeEmpresa = "Capcom", EstaAtivo = true };
            context.Empresa.AddRange(sony, capcom);

            var gow = new Jogo { Titulo = "God of War", Descricao = "Action", Imagem = "gow.jpg", DataLancamento = new DateOnly(2018, 4, 20), Empresa = sony, EstaAtivo = true };
            var re4 = new Jogo { Titulo = "Resident Evil 4", Descricao = "Horror", Imagem = "re4.jpg", DataLancamento = new DateOnly(2023, 3, 24), Empresa = capcom, EstaAtivo = true };
            context.Jogos.AddRange(gow, re4);

            var user = new Usuario { NomeUsuario = "Reviewer", Email = "rev@gamelog.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.Add(user);
            await context.SaveChangesAsync();

            // GOW recebe nota 5, RE4 recebe nota 3
            context.Avaliacoes.Add(new Avaliacao { Jogo = gow, Usuario = user, Nota = 5, EstaAtivo = true });
            context.Avaliacoes.Add(new Avaliacao { Jogo = re4, Usuario = user, Nota = 3, EstaAtivo = true });
            await context.SaveChangesAsync();

            // Act 1: Filtrar apenas nota 5
            var resNota5 = await service.ListarJogosPaginados(notaMinima: 5);
            resNota5.Itens.Should().ContainSingle(j => j.Titulo == "God of War");

            // Act 2: Filtrar apenas Capcom
            var resCapcom = await service.ListarJogosPaginados(empresa: "Capcom");
            resCapcom.Itens.Should().ContainSingle(j => j.Titulo == "Resident Evil 4");
        }

        [Fact]
        public async Task ListarDestaquesHeroAsync_DeveRetornarJogosComCapas()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var rawgService = CreateMockRawgService(context);
            var service = new JogoServices(context, rawgService);

            var empresa = new Empresa { NomeEmpresa = "CD Projekt Red", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogo = new Jogo
            {
                Titulo = "The Witcher 3: Wild Hunt",
                Descricao = "Geralt de Rivia",
                Imagem = "witcher.jpg",
                DataLancamento = new DateOnly(2015, 5, 18),
                Empresa = empresa,
                EstaAtivo = true
            };
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            // Act
            var destaques = (await service.ListarDestaquesHeroAsync(5)).ToList();

            // Assert
            destaques.Should().NotBeEmpty();
            destaques.First().Titulo.Should().Be("The Witcher 3: Wild Hunt");
        }
    }
}
