using System;
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
    public class BuscaGlobalControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public BuscaGlobalControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Buscar_ComQueryVazia_DeveRetornar200OKComListasVazias()
        {
            // Act
            var response = await _client.GetAsync("/api/BuscaGlobal?q=");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BuscaGlobalDTO>();
            result.Should().NotBeNull();
            result!.Jogos.Should().BeEmpty();
        }

        [Fact]
        public async Task Buscar_ComTermoValido_DeveRetornar200OKEResultados()
        {
            // Arrange
            var jogoId = Guid.NewGuid();
            var empresaId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                var empresa = new Empresa
                {
                    Id = empresaId,
                    NomeEmpresa = "GlobalSearch Studio",
                    EstaAtivo = true
                };

                context.Jogos.Add(new Jogo
                {
                    Id = jogoId,
                    Titulo = "Cyberpunk Odyssey 2099",
                    DataLancamento = new DateOnly(2023, 1, 1),
                    EstaAtivo = true,
                    Empresa = empresa
                });
                await context.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync("/api/BuscaGlobal?q=cyberpunk");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<BuscaGlobalDTO>();
            result.Should().NotBeNull();
            result!.Termo.Should().Be("cyberpunk");
            result.Jogos.Should().Contain(j => j.Titulo.Contains("Cyberpunk"));
        }
    }
}
