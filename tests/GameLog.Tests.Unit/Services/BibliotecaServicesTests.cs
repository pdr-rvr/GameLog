using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class BibliotecaServicesTests
    {
        [Fact]
        public async Task SalvarItemBiblioteca_NovoItem_DeveSalvarStatusCorretamente()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new BibliotecaServices(context);

            var usuario = new Usuario { NomeUsuario = "Gamer1", Email = "g1@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo = new Jogo { Titulo = "Cyberpunk 2077", Descricao = "RPG", Imagem = "cp77.jpg", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            var dto = new SalvarItemBibliotecaDTO
            {
                JogoId = jogo.Id,
                Status = StatusJogo.Jogando
            };

            // Act
            var result = await service.SalvarItemBiblioteca(usuario.Id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be((int)StatusJogo.Jogando);
            result.StatusNome.Should().Be("Jogando");
            result.TituloJogo.Should().Be("Cyberpunk 2077");

            var stats = await service.ObterEstatisticasBiblioteca(usuario.Id);
            stats.TotalJogos.Should().Be(1);
            stats.TotalJogando.Should().Be(1);
            stats.TotalZerados.Should().Be(0);
        }

        [Fact]
        public async Task SalvarItemBiblioteca_AtualizarParaZerado_DeveDefinirDataConclusao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new BibliotecaServices(context);

            var usuario = new Usuario { NomeUsuario = "Gamer2", Email = "g2@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo = new Jogo { Titulo = "God of War", Descricao = "Action", Imagem = "gow.jpg", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            await service.SalvarItemBiblioteca(usuario.Id, new SalvarItemBibliotecaDTO { JogoId = jogo.Id, Status = StatusJogo.Jogando });

            // Act: Atualizar para Zerado
            var result = await service.SalvarItemBiblioteca(usuario.Id, new SalvarItemBibliotecaDTO { JogoId = jogo.Id, Status = StatusJogo.Zerado });

            // Assert
            result.StatusNome.Should().Be("Zerado");
            result.DataConclusao.Should().NotBeNull();
        }

        [Fact]
        public async Task SalvarJogosFavoritos_PodioTop5_DevePersistirComSucesso()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new BibliotecaServices(context);

            var empresa = new Empresa { NomeEmpresa = "Nintendo", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var usuario = new Usuario { NomeUsuario = "PodiumUser", Email = "podium@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo1 = new Jogo { Titulo = "Game 1", Descricao = "RPG", Imagem = "g1.jpg", Empresa = empresa, EstaAtivo = true };
            var jogo2 = new Jogo { Titulo = "Game 2", Descricao = "RPG", Imagem = "g2.jpg", Empresa = empresa, EstaAtivo = true };
            context.Usuarios.Add(usuario);
            context.Jogos.AddRange(jogo1, jogo2);
            await context.SaveChangesAsync();

            var favoritosDto = new SalvarJogosFavoritosDTO
            {
                Favoritos = new List<ItemFavoritoPosicaoDTO>
                {
                    new ItemFavoritoPosicaoDTO { Posicao = 1, JogoId = jogo1.Id },
                    new ItemFavoritoPosicaoDTO { Posicao = 2, JogoId = jogo2.Id }
                }
            };

            // Act
            var result = await service.SalvarJogosFavoritos(usuario.Id, favoritosDto);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(f => f.Posicao == 1 && f.TituloJogo == "Game 1");
            result.Should().Contain(f => f.Posicao == 2 && f.TituloJogo == "Game 2");
        }

        [Fact]
        public async Task SalvarItemBiblioteca_JogoNaoLancado_ComStatusDiferenteDeQueroJogar_DeveLancarInvalidOperationException()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new BibliotecaServices(context);

            var usuario = new Usuario { NomeUsuario = "FutureGamer", Email = "future@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogoFuturo = new Jogo
            {
                Titulo = "Wolverine",
                Descricao = "Marvel's Wolverine",
                Imagem = "wolverine.jpg",
                DataLancamento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            context.Jogos.Add(jogoFuturo);
            await context.SaveChangesAsync();

            var dto = new SalvarItemBibliotecaDTO
            {
                JogoId = jogoFuturo.Id,
                Status = StatusJogo.Jogando // Inválido para jogos não lançados
            };

            // Act & Assert
            var act = async () => await service.SalvarItemBiblioteca(usuario.Id, dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Jogos ainda não lançados só podem ser adicionados à biblioteca com o status 'Quero Jogar'*");
        }

        [Fact]
        public async Task SalvarItemBiblioteca_JogoNaoLancado_ComStatusQueroJogar_DeveSalvarComSucesso()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new BibliotecaServices(context);

            var usuario = new Usuario { NomeUsuario = "FutureGamer2", Email = "future2@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogoFuturo = new Jogo
            {
                Titulo = "The Witcher 4",
                Descricao = "Polaris",
                Imagem = "tw4.jpg",
                DataLancamento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            context.Jogos.Add(jogoFuturo);
            await context.SaveChangesAsync();

            var dto = new SalvarItemBibliotecaDTO
            {
                JogoId = jogoFuturo.Id,
                Status = StatusJogo.QueroJogar
            };

            // Act
            var result = await service.SalvarItemBiblioteca(usuario.Id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be((int)StatusJogo.QueroJogar);
            result.StatusNome.Should().Be("Quero Jogar");
        }
    }
}
