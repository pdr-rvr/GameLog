using Microsoft.IdentityModel.Tokens;
using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task AutenticarUsuario_ComCredenciaisValidas_DeveRetornarTokenERefreshToken()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var authService = new AuthService(context, mapper, jwtOptions);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            var usuario = new Usuario
            {
                NomeUsuario = "GamerAuth",
                Email = "gamerauth@gamelog.com",
                Senha = senhaHash,
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginDto = new UsuarioLoginDTO
            {
                Email = "gamerauth@gamelog.com",
                Senha = "Password123!"
            };

            // Act
            var result = await authService.AutenticarUsuario(loginDto, "127.0.0.1");

            // Assert
            result.Sucesso.Should().BeTrue();
            result.Usuario.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Usuario!.Email.Should().Be("gamerauth@gamelog.com");

            var tokenDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
            tokenDb.Should().NotBeNull();
            tokenDb!.UsuarioId.Should().Be(usuario.Id);
            tokenDb.RevogadoEm.Should().BeNull();
            tokenDb.CriadoPorIp.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task AutenticarUsuario_ComCredenciaisInvalidas_DeveRetornarNulo()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var authService = new AuthService(context, mapper, jwtOptions);

            var loginDto = new UsuarioLoginDTO
            {
                Email = "naoexiste@gamelog.com",
                Senha = "Password123!"
            };

            // Act
            var result = await authService.AutenticarUsuario(loginDto, "127.0.0.1");

            // Assert
            result.Sucesso.Should().BeFalse();
            result.Usuario.Should().BeNull();
            result.Token.Should().BeNull();
            result.RefreshToken.Should().BeNull();
        }

        [Fact]
        public async Task RenovarTokenAsync_ComTokenValido_DeveRotacionarERevogarTokenAnterior()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var authService = new AuthService(context, mapper, jwtOptions);

            var usuario = new Usuario
            {
                NomeUsuario = "GamerRotate",
                Email = "rotate@gamelog.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginResult = await authService.AutenticarUsuario(new UsuarioLoginDTO
            {
                Email = "rotate@gamelog.com",
                Senha = "Password123!"
            }, "127.0.0.1");

            var oldToken = loginResult.RefreshToken!;

            // Act
            var renewResult = await authService.RenovarTokenAsync(oldToken, "127.0.0.2");

            // Assert
            renewResult.Sucesso.Should().BeTrue();
            renewResult.Usuario.Should().NotBeNull();
            renewResult.Token.Should().NotBeNullOrEmpty();
            renewResult.RefreshToken.Should().NotBeNullOrEmpty();
            renewResult.RefreshToken.Should().NotBe(oldToken);

            var oldTokenDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldToken);
            oldTokenDb.Should().NotBeNull();
            oldTokenDb!.RevogadoEm.Should().NotBeNull();
            oldTokenDb.SubstituidoPorToken.Should().Be(renewResult.RefreshToken);
            oldTokenDb.RevogadoPorIp.Should().Be("127.0.0.2");

            var newTokenDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == renewResult.RefreshToken);
            newTokenDb.Should().NotBeNull();
            newTokenDb!.RevogadoEm.Should().BeNull();
            newTokenDb.CriadoPorIp.Should().Be("127.0.0.2");
        }

        [Fact]
        public async Task RenovarTokenAsync_ComTokenJaRevogado_DeveDetectarReusoEInvalidarTodasSessoes()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var authService = new AuthService(context, mapper, jwtOptions);

            var usuario = new Usuario
            {
                NomeUsuario = "GamerReplay",
                Email = "replay@gamelog.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginResult = await authService.AutenticarUsuario(new UsuarioLoginDTO
            {
                Email = "replay@gamelog.com",
                Senha = "Password123!"
            }, "127.0.0.1");

            var initialToken = loginResult.RefreshToken!;

            // Primeira renovação legítima
            var firstRenewal = await authService.RenovarTokenAsync(initialToken, "127.0.0.1");
            var legitNewToken = firstRenewal.RefreshToken;

            // Act & Assert: Tentativa de replay com o token antigo deve lançar SecurityTokenException
            await Assert.ThrowsAsync<SecurityTokenException>(() => authService.RenovarTokenAsync(initialToken, "192.168.1.100"));

            // Ambos os tokens devem agora estar revogados
            var initialDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == initialToken);
            var legitDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == legitNewToken);

            initialDb!.RevogadoEm.Should().NotBeNull();
            legitDb!.RevogadoEm.Should().NotBeNull();
        }

        [Fact]
        public async Task RevogarTokenAsync_ComTokenValido_DeveRevogarCorretamente()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var authService = new AuthService(context, mapper, jwtOptions);

            var usuario = new Usuario
            {
                NomeUsuario = "GamerRevoke",
                Email = "revoke@gamelog.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                EstaAtivo = true
            };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginResult = await authService.AutenticarUsuario(new UsuarioLoginDTO
            {
                Email = "revoke@gamelog.com",
                Senha = "Password123!"
            }, "127.0.0.1");

            // Act
            var revoked = await authService.RevogarTokenAsync(loginResult.RefreshToken!, "127.0.0.1");

            // Assert
            revoked.Should().BeTrue();
            var tokenDb = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == loginResult.RefreshToken);
            tokenDb.Should().NotBeNull();
            tokenDb!.RevogadoEm.Should().NotBeNull();
        }
    }
}
