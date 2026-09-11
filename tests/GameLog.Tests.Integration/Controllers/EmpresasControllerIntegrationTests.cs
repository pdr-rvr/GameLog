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
    public class EmpresasControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public EmpresasControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ListarTodasEmpresas_DeveRetornar200OK()
        {
            // Act
            var response = await _client.GetAsync("/api/Empresas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ObterEmpresaPorId_QuandoExiste_DeveRetornar200OK()
        {
            // Arrange
            var empresaId = Guid.NewGuid();

            using (var context = _factory.CreateDbContext())
            {
                context.Empresa.Add(new Empresa
                {
                    Id = empresaId,
                    NomeEmpresa = "Valve Corporation",
                    EstaAtivo = true
                });
                await context.SaveChangesAsync();
            }

            // Act
            var response = await _client.GetAsync($"/api/Empresas/{empresaId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var empresa = await response.Content.ReadFromJsonAsync<EmpresaDTO>();
            empresa.Should().NotBeNull();
            empresa!.NomeEmpresa.Should().Be("Valve Corporation");
        }

        [Fact]
        public async Task ObterEmpresaPorId_QuandoNaoExiste_DeveRetornar404NotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/Empresas/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ListarJogosPorEmpresa_DeveRetornar200OK()
        {
            // Arrange
            var empresaId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Empresas/{empresaId}/jogos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
