using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.DTOs;
using Xunit;

namespace GameLog.Tests.Integration.Controllers
{
    public class ComunidadeControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ComunidadeControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ObterTendencias_DeveRetornar200OKComDadosAgregados()
        {
            // Act
            var response = await _client.GetAsync("/api/Comunidade/tendencias");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TendenciasComunidadeDTO>();
            result.Should().NotBeNull();
            result!.JogosMaisDiscutidos.Should().NotBeNull();
            result.AvaliacoesMaisCurtidas.Should().NotBeNull();
            result.ListasEmDestaque.Should().NotBeNull();
        }
    }
}
