using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Xunit;

namespace GameLog.Tests.Integration.Controllers
{
    public class JogosControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public JogosControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ListarTodosJogos_DeveRetornar200OK()
        {
            // Act
            var response = await _client.GetAsync("/api/Jogos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ListarJogosPaginados_DeveRetornar200OKComListaPaginada()
        {
            // Act
            var response = await _client.GetAsync("/api/Jogos?pagina=1&itensPorPagina=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ObterJogoPorId_QuandoExiste_DeveRetornar200OK()
        {
            // Arrange
            var jogoId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                var empresa = new Empresa
                {
                    Id = Guid.NewGuid(),
                    NomeEmpresa = "Studio Integration Test",
                    EstaAtivo = true
                };

                context.Jogos.Add(new Jogo
                {
                    Id = jogoId,
                    Titulo = "Integration Test Game",
                    Descricao = "Game for testing",
                    DataLancamento = new DateOnly(2023, 5, 20),
                    EstaAtivo = true,
                    Imagem = "/covers/test.jpg",
                    Empresa = empresa
                });
                await context.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync($"/api/Jogos/{jogoId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var jogo = await response.Content.ReadFromJsonAsync<JogoDTO>();
            jogo.Should().NotBeNull();
            jogo!.Titulo.Should().Be("Integration Test Game");
        }

        [Fact]
        public async Task ObterJogoPorId_QuandoNaoExiste_DeveRetornar404NotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/Jogos/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
