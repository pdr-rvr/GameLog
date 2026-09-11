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
    public class ListasControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ListasControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarLista_SemAutenticacao_DeveRetornar401Unauthorized()
        {
            // Arrange
            var dto = new CriarListaDTO
            {
                Titulo = "Minha Lista",
                Descricao = "Coleção de teste",
                EstaPublica = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Listas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CriarLista_Autenticado_DeveRetornar201CreatedEObjetoCriado()
        {
            // Arrange
            var userId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                context.Usuarios.Add(new Usuario
                {
                    Id = userId,
                    NomeUsuario = $"ListUser_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    Email = $"list_{Guid.NewGuid().ToString("N").Substring(0, 8)}@test.com",
                    Senha = "hash_fake_bcrypt",
                    EstaAtivo = true
                });
                await context.SaveChangesAsync();
            }

            var authClient = _factory.CreateAuthenticatedClient(userId);

            var dto = new CriarListaDTO
            {
                Titulo = "Melhores RPGs de Todos os Tempos",
                Descricao = "Uma seleção pessoal dos melhores RPGs",
                EstaPublica = true,
                JogosIds = new List<Guid>()
            };

            // Act
            var response = await authClient.PostAsJsonAsync("/api/Listas", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var lista = await response.Content.ReadFromJsonAsync<ListaDeJogosDTO>();
            lista.Should().NotBeNull();
            lista!.Titulo.Should().Be("Melhores RPGs de Todos os Tempos");
        }

        [Fact]
        public async Task ObterListaPorId_QuandoNaoExiste_DeveRetornar404NotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/Listas/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
