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
    public class AvaliacoesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AvaliacoesControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarAvaliacao_SemAutenticacao_DeveRetornar401Unauthorized()
        {
            // Arrange
            var dto = new CriarAvaliacaoDTO
            {
                JogoId = Guid.NewGuid(),
                Nota = 5,
                TextoAvaliacao = "Excelente jogo!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Avaliacoes", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CriarAvaliacao_ComNotaForaDoIntervalo_DeveRetornar400ProblemDetails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authClient = _factory.CreateAuthenticatedClient(userId);

            var dto = new CriarAvaliacaoDTO
            {
                JogoId = Guid.NewGuid(),
                Nota = 10, // Inválido (deve ser 1 a 5)
                TextoAvaliacao = "Nota inválida"
            };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/Avaliacoes", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("nota");
        }

        [Fact]
        public async Task CriarAvaliacao_AutenticadoComDadosValidos_DeveRetornar201Created()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                var empresa = new Empresa
                {
                    Id = Guid.NewGuid(),
                    NomeEmpresa = "Review Test Studio",
                    EstaAtivo = true
                };

                context.Usuarios.Add(new Usuario
                {
                    Id = userId,
                    NomeUsuario = $"User_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    Email = $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}@test.com",
                    Senha = "hash_fake_bcrypt",
                    EstaAtivo = true
                });

                context.Jogos.Add(new Jogo
                {
                    Id = jogoId,
                    Titulo = "Game for Review",
                    DataLancamento = new DateOnly(2023, 1, 1),
                    EstaAtivo = true,
                    Empresa = empresa
                });

                await context.SaveChangesAsync();
            }

            var authClient = _factory.CreateAuthenticatedClient(userId);

            var dto = new CriarAvaliacaoDTO
            {
                JogoId = jogoId,
                Nota = 5,
                TextoAvaliacao = "Jogo espetacular!"
            };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/Avaliacoes", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<AvaliacaoDTO>();
            result.Should().NotBeNull();
            result!.Nota.Should().Be(5);
            result.TextoAvaliacao.Should().Be("Jogo espetacular!");
        }

        [Fact]
        public async Task ListarAvaliacoes_DeveRetornar200OK()
        {
            // Act
            var response = await _client.GetAsync("/api/Avaliacoes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ListarAvaliacoes_ComFiltroDeNota_DeveRetornar200OK()
        {
            // Act
            var response = await _client.GetAsync("/api/Avaliacoes?nota=5&ordenacao=curtidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<AvaliacaoDTO>>();
            result.Should().NotBeNull();
        }
    }
}
