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
    public class BibliotecaControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public BibliotecaControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SalvarItem_SemAutenticacao_DeveRetornar401Unauthorized()
        {
            // Arrange
            var dto = new SalvarItemBibliotecaDTO
            {
                JogoId = Guid.NewGuid(),
                Status = StatusJogo.Jogando
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Biblioteca", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SalvarItem_Autenticado_DeveSalvarERetornar200OK()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                var empresa = new Empresa
                {
                    Id = Guid.NewGuid(),
                    NomeEmpresa = "Bib Test Studio",
                    EstaAtivo = true
                };

                context.Usuarios.Add(new Usuario
                {
                    Id = userId,
                    NomeUsuario = $"BibUser_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    Email = $"bib_{Guid.NewGuid().ToString("N").Substring(0, 8)}@test.com",
                    Senha = "hash_fake_bcrypt",
                    EstaAtivo = true
                });

                context.Jogos.Add(new Jogo
                {
                    Id = jogoId,
                    Titulo = "Library Test Game",
                    DataLancamento = new DateOnly(2023, 1, 1),
                    EstaAtivo = true,
                    Empresa = empresa
                });

                await context.SaveChangesAsync();
            }

            var authClient = _factory.CreateAuthenticatedClient(userId);

            var dto = new SalvarItemBibliotecaDTO
            {
                JogoId = jogoId,
                Status = StatusJogo.Jogando
            };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/Biblioteca", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var item = await response.Content.ReadFromJsonAsync<ItemBibliotecaDTO>();
            item.Should().NotBeNull();
            item!.Status.Should().Be((int)StatusJogo.Jogando);
        }

        [Fact]
        public async Task ObterBibliotecaUsuario_DeveRetornar200OK()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Biblioteca/usuario/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
