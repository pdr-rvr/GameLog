using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class AvaliacaoServicesTests
    {
        [Fact]
        public async Task CriarAvaliacao_ComDadosValidos_DeveSalvarComSucesso()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "FromSoftware", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogo = new Jogo
            {
                Titulo = "Elden Ring",
                Descricao = "RPG de Ação",
                Imagem = "elden_ring.jpg",
                DataLancamento = new DateOnly(2022, 2, 25),
                Empresa = empresa,
                EstaAtivo = true
            };
            context.Jogos.Add(jogo);

            var usuario = new Usuario
            {
                NomeUsuario = "Tarnished",
                Email = "tarnished@landsbetween.com",
                Senha = "hashedPassword",
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new CriarAvaliacaoDTO
            {
                JogoId = jogo.Id,
                Nota = 5,
                TextoAvaliacao = "Obra-prima absoluta!"
            };

            // Act
            var result = await service.CriarAvaliacao(dto, usuario.Id);

            // Assert
            result.Should().NotBeNull();
            result.Nota.Should().Be(5);
            result.NomeJogo.Should().Be("Elden Ring");
            result.NomeUsuario.Should().Be("Tarnished");
            result.TextoAvaliacao.Should().Be("Obra-prima absoluta!");
        }

        [Fact]
        public async Task CriarAvaliacao_ComNotaInvalida_DeveLancarArgumentException()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var dto = new CriarAvaliacaoDTO
            {
                JogoId = Guid.NewGuid(),
                Nota = 6, // Nota inválida > 5
                TextoAvaliacao = "Nota fora do padrão"
            };

            // Act & Assert
            var act = async () => await service.CriarAvaliacao(dto, Guid.NewGuid());
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*entre 1 e 5 estrelas*");
        }

        [Fact]
        public async Task CriarAvaliacao_DuplicadaParaMesmoJogo_DeveLancarInvalidOperationException()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "FromSoftware", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogo = new Jogo { Titulo = "Dark Souls", Descricao = "Action RPG", Imagem = "ds.jpg", Empresa = empresa, EstaAtivo = true };
            context.Jogos.Add(jogo);

            var usuario = new Usuario { NomeUsuario = "AshenOne", Email = "ashen@gamelog.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new CriarAvaliacaoDTO { JogoId = jogo.Id, Nota = 5, TextoAvaliacao = "Primeira avaliação" };
            await service.CriarAvaliacao(dto, usuario.Id);

            // Act & Assert (Segunda avaliação para o mesmo jogo)
            var act = async () => await service.CriarAvaliacao(dto, usuario.Id);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*já possui uma avaliação ativa*");
        }

        [Fact]
        public async Task AlternarCurtida_DeveIncrementarEDecrementarCurtidas()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "FromSoftware", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var autor = new Usuario { NomeUsuario = "Author", Email = "author@gamelog.com", Senha = "hash", EstaAtivo = true };
            var leitor = new Usuario { NomeUsuario = "Reader", Email = "reader@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo = new Jogo { Titulo = "Sekiro", Descricao = "Action", Imagem = "sekiro.jpg", Empresa = empresa, EstaAtivo = true };

            context.Usuarios.AddRange(autor, leitor);
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            var avaliacao = await service.CriarAvaliacao(new CriarAvaliacaoDTO
            {
                JogoId = jogo.Id,
                Nota = 5,
                TextoAvaliacao = "Excelente combate"
            }, autor.Id);

            // Act 1: Curtir
            var (curtido1, total1) = await service.AlternarCurtida(avaliacao.AvaliacaoId, leitor.Id);
            curtido1.Should().BeTrue();
            total1.Should().Be(1);

            // Act 2: Descurtir (toggle)
            var (curtido2, total2) = await service.AlternarCurtida(avaliacao.AvaliacaoId, leitor.Id);
            curtido2.Should().BeFalse();
            total2.Should().Be(0);
        }

        [Fact]
        public async Task AlternarCurtida_NaPropriaAvaliacao_DeveLancarExcecao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "Sony", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var autor = new Usuario { NomeUsuario = "SelfLover", Email = "self@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo = new Jogo { Titulo = "Bloodborne", Descricao = "Action RPG", Imagem = "bb.jpg", Empresa = empresa, EstaAtivo = true };
            context.Usuarios.Add(autor);
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            var avaliacao = await service.CriarAvaliacao(new CriarAvaliacaoDTO
            {
                JogoId = jogo.Id,
                Nota = 5,
                TextoAvaliacao = "Masterpiece"
            }, autor.Id);

            // Act & Assert: Autor tentando curtir a própria review
            var act = async () => await service.AlternarCurtida(avaliacao.AvaliacaoId, autor.Id);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*não pode curtir sua própria avaliação*");
        }

        [Fact]
        public async Task AdicionarResposta_DeveSalvarComentarioNaThread()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "Team Cherry", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var autor = new Usuario { NomeUsuario = "Author", Email = "author@gamelog.com", Senha = "hash", EstaAtivo = true };
            var comentarista = new Usuario { NomeUsuario = "Commenter", Email = "commenter@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo = new Jogo { Titulo = "Hollow Knight", Descricao = "Metroidvania", Imagem = "hk.jpg", Empresa = empresa, EstaAtivo = true };

            context.Usuarios.AddRange(autor, comentarista);
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            var avaliacao = await service.CriarAvaliacao(new CriarAvaliacaoDTO
            {
                JogoId = jogo.Id,
                Nota = 5,
                TextoAvaliacao = "Melhor metroidvania de todos os tempos"
            }, autor.Id);

            // Act
            var resposta = await service.AdicionarResposta(avaliacao.AvaliacaoId, comentarista.Id, new CriarRespostaDTO
            {
                Comentario = "Concordo plenamente!"
            });

            // Assert
            resposta.Should().NotBeNull();
            resposta.Comentario.Should().Be("Concordo plenamente!");
            resposta.NomeUsuario.Should().Be("Commenter");

            var respostas = await service.ListarRespostasPorAvaliacao(avaliacao.AvaliacaoId);
            respostas.Should().ContainSingle(r => r.Comentario == "Concordo plenamente!");
        }

        [Fact]
        public async Task CriarAvaliacao_JogoNaoLancado_DeveLancarInvalidOperationException()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = new AvaliacaoServices(context, mapper);

            var empresa = new Empresa { NomeEmpresa = "Rockstar Games", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogoFuturo = new Jogo
            {
                Titulo = "Grand Theft Auto VI",
                Descricao = "Próximo grande título",
                Imagem = "gta6.jpg",
                DataLancamento = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                Empresa = empresa,
                EstaAtivo = true
            };
            context.Jogos.Add(jogoFuturo);

            var usuario = new Usuario
            {
                NomeUsuario = "HypedGamer",
                Email = "hyped@gamelog.com",
                Senha = "hash",
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new CriarAvaliacaoDTO
            {
                JogoId = jogoFuturo.Id,
                Nota = 5,
                TextoAvaliacao = "Já sei que vai ser 10/10!"
            };

            // Act & Assert
            var act = async () => await service.CriarAvaliacao(dto, usuario.Id);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Não é permitido avaliar ou dar nota a um jogo que ainda não foi lançado*");
        }
    }
}
