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
    public class UsuariosControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsuariosControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CriarUsuario_ComDadosValidos_DeveRetornar201CreatedEObjetoCriado()
        {
            // Arrange
            var novoUsuario = new CriarUsuarioDTO
            {
                NomeUsuario = $"User_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Email = $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}@test.com",
                Senha = "SenhaForte123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Usuarios/registrar", novoUsuario);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var usuario = await response.Content.ReadFromJsonAsync<UsuarioDTO>();
            usuario.Should().NotBeNull();
            usuario!.NomeUsuario.Should().Be(novoUsuario.NomeUsuario);
            usuario.Email.Should().Be(novoUsuario.Email);
            usuario.UsuarioId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CriarUsuario_ComEmailInvalido_DeveRetornar400ProblemDetails()
        {
            // Arrange
            var novoUsuario = new CriarUsuarioDTO
            {
                NomeUsuario = "ValidUser",
                Email = "email-invalido-sem-arroba",
                Senha = "SenhaForte123!@#"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Usuarios/registrar", novoUsuario);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("email");
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornar200ETokenJwt()
        {
            // Arrange
            var email = $"login_{Guid.NewGuid().ToString("N").Substring(0, 8)}@test.com";
            var senha = "SenhaForte123!@#" ;

            var registroDto = new CriarUsuarioDTO
            {
                NomeUsuario = $"LoginUser_{Guid.NewGuid().ToString("N").Substring(0, 6)}",
                Email = email,
                Senha = senha
            };
            await _client.PostAsJsonAsync("/api/Usuarios/registrar", registroDto);

            var loginDto = new UsuarioLoginDTO
            {
                Email = email,
                Senha = senha
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Usuarios/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("token");
        }

        [Fact]
        public async Task Login_ComSenhaIncorreta_DeveRetornar401Unauthorized()
        {
            // Arrange
            var loginDto = new UsuarioLoginDTO
            {
                Email = "inexistente@test.com",
                Senha = "SenhaErrada123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Usuarios/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ObterUsuarioPorId_QuandoNaoExiste_DeveRetornar404NotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/Usuarios/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
