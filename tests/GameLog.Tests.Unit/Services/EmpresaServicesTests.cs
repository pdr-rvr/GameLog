using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using GameLog.Tests.Unit.Helpers;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class EmpresaServicesTests
    {
        [Fact]
        public async Task ListarEmpresas_DeveRetornarEmpresasComTotaisEMedias()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var dev = new Empresa { Id = Guid.NewGuid(), NomeEmpresa = "FromSoftware", EstaAtivo = true };
            var pub = new Empresa { Id = Guid.NewGuid(), NomeEmpresa = "Bandai Namco", EstaAtivo = true };
            context.Empresa.AddRange(dev, pub);

            var jogo = new Jogo
            {
                Id = Guid.NewGuid(),
                Titulo = "Elden Ring",
                Descricao = "RPG de Ação",
                Imagem = "https://img.com/elden.jpg",
                DataLancamento = new DateOnly(2022, 2, 25),
                ClassificacaoIndicativa = 16,
                Empresa = dev,
                Publicadora = pub,
                EstaAtivo = true
            };
            context.Jogos.Add(jogo);

            var user = new Usuario { Id = Guid.NewGuid(), NomeUsuario = "Gamer", Email = "g@test.com", Senha = "123", EstaAtivo = true };
            context.Usuarios.Add(user);

            var avaliacao = new Avaliacao
            {
                Id = Guid.NewGuid(),
                Jogo = jogo,
                Usuario = user,
                Nota = 5,
                TextoAvaliacao = "Incrível",
                DataPublicacao = DateTime.UtcNow,
                EstaAtivo = true
            };
            context.Avaliacoes.Add(avaliacao);
            await context.SaveChangesAsync();

            var service = new EmpresaServices(context);

            // Act
            var result = (await service.ListarEmpresas()).ToList();

            // Assert
            result.Should().HaveCount(2);
            var devResult = result.FirstOrDefault(e => e.EmpresaId == dev.Id);
            devResult.Should().NotBeNull();
            devResult!.TotalJogos.Should().Be(1);
            devResult.MediaNotasJogos.Should().Be(5.0);

            var pubResult = result.FirstOrDefault(e => e.EmpresaId == pub.Id);
            pubResult.Should().NotBeNull();
            pubResult!.TotalJogos.Should().Be(1);
            pubResult.MediaNotasJogos.Should().Be(5.0);
        }

        [Fact]
        public async Task ObterEmpresaPorId_QuandoExiste_DeveRetornarEmpresaComEstatisticas()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var emp = new Empresa { Id = Guid.NewGuid(), NomeEmpresa = "Sony Interactive Entertainment", EstaAtivo = true };
            context.Empresa.Add(emp);

            var jogo = new Jogo
            {
                Id = Guid.NewGuid(),
                Titulo = "God of War",
                Descricao = "Ação",
                Imagem = "https://img.com/gow.jpg",
                DataLancamento = new DateOnly(2018, 4, 20),
                ClassificacaoIndicativa = 18,
                Empresa = emp,
                EstaAtivo = true
            };
            context.Jogos.Add(jogo);
            await context.SaveChangesAsync();

            var service = new EmpresaServices(context);

            // Act
            var result = await service.ObterEmpresaPorId(emp.Id);

            // Assert
            result.Should().NotBeNull();
            result!.NomeEmpresa.Should().Be("Sony Interactive Entertainment");
            result.TotalJogos.Should().Be(1);
        }

        [Fact]
        public async Task ObterEmpresaPorId_QuandoNaoExiste_DeveRetornarNull()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new EmpresaServices(context);

            // Act
            var result = await service.ObterEmpresaPorId(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ListarJogosPorEmpresa_DeveRetornarJogosOrdenadosPorMedia()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var emp = new Empresa { Id = Guid.NewGuid(), NomeEmpresa = "Rockstar Games", EstaAtivo = true };
            context.Empresa.Add(emp);

            var genero = new Genero { Id = Guid.NewGuid(), TituloGenero = "Ação", EstaAtivo = true };
            context.Generos.Add(genero);

            var jogo1 = new Jogo
            {
                Id = Guid.NewGuid(),
                Titulo = "GTA V",
                Descricao = "Mundo aberto",
                Imagem = "https://img.com/gta.jpg",
                DataLancamento = new DateOnly(2013, 9, 17),
                ClassificacaoIndicativa = 18,
                Empresa = emp,
                EstaAtivo = true,
                Generos = new List<Genero> { genero }
            };

            var jogo2 = new Jogo
            {
                Id = Guid.NewGuid(),
                Titulo = "Red Dead Redemption 2",
                Descricao = "Velho Oeste",
                Imagem = "https://img.com/rdr2.jpg",
                DataLancamento = new DateOnly(2018, 10, 26),
                ClassificacaoIndicativa = 18,
                Empresa = emp,
                EstaAtivo = true,
                Generos = new List<Genero> { genero }
            };

            context.Jogos.AddRange(jogo1, jogo2);

            var user = new Usuario { Id = Guid.NewGuid(), NomeUsuario = "Gamer", Email = "g@test.com", Senha = "123", EstaAtivo = true };
            context.Usuarios.Add(user);

            var av1 = new Avaliacao { Id = Guid.NewGuid(), Jogo = jogo1, Usuario = user, Nota = 4, EstaAtivo = true };
            var av2 = new Avaliacao { Id = Guid.NewGuid(), Jogo = jogo2, Usuario = user, Nota = 5, EstaAtivo = true };
            context.Avaliacoes.AddRange(av1, av2);
            await context.SaveChangesAsync();

            var service = new EmpresaServices(context);

            // Act
            var result = (await service.ListarJogosPorEmpresa(emp.Id)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Titulo.Should().Be("Red Dead Redemption 2");
            result[0].MediaAvaliacoes.Should().Be(5.0);
            result[1].Titulo.Should().Be("GTA V");
            result[1].MediaAvaliacoes.Should().Be(4.0);
        }
    }
}
