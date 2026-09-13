using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.Database;
using GameLog_Backend.Entities;
using GameLog_Backend.Helpers;
using GameLog_Backend.Interceptors;
using GameLog_Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GameLog.Tests.Unit.Interceptors
{
    public class AuditSaveChangesInterceptorTests
    {
        private (GameLogContext context, Mock<IHttpContextAccessor> mockAccessor) CreateContextWithInterceptor(
            string? userId = null,
            string? ipAddress = null,
            string? correlationId = null)
        {
            var mockAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();

            if (!string.IsNullOrEmpty(userId))
            {
                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                httpContext.User = new ClaimsPrincipal(identity);
            }

            if (!string.IsNullOrEmpty(ipAddress))
            {
                httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);
            }

            if (!string.IsNullOrEmpty(correlationId))
            {
                httpContext.Items[CorrelationIdMiddleware.CorrelationIdHeaderName] = correlationId;
            }

            mockAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var interceptor = new AuditSaveChangesInterceptor(mockAccessor.Object);

            var options = new DbContextOptionsBuilder<GameLogContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            var context = new GameLogContext(options);
            return (context, mockAccessor);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEntityAdded_ShouldCreateInsertAuditLog()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var correlationId = "corr-test-123";
            var (context, _) = CreateContextWithInterceptor(userId: userId, ipAddress: "192.168.1.100", correlationId: correlationId);

            var usuario = new Usuario
            {
                NomeUsuario = "Pedro",
                Email = "pedro@example.com",
                Senha = "secret_hash_value"
            };

            // Act
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Assert
            var auditLogs = await context.AuditLogs.ToListAsync();
            auditLogs.Should().HaveCount(1);

            var audit = auditLogs.First();
            audit.Entidade.Should().Be(nameof(Usuario));
            audit.EntidadeId.Should().Be(usuario.Id.ToString());
            audit.TipoAcao.Should().Be("INSERT");
            audit.UsuarioId.Should().Be(userId);
            audit.IpAddress.Should().Be("192.168.1.100");
            audit.CorrelationId.Should().Be(correlationId);
            audit.ValoresAntigos.Should().BeNull();
            audit.ValoresNovos.Should().NotBeNull();

            var novos = JsonSerializer.Deserialize<Dictionary<string, object?>>(audit.ValoresNovos!);
            novos.Should().ContainKey("Email");
            novos!["Email"]?.ToString().Should().Be("pedro@example.com");
            novos["Senha"]?.ToString().Should().Be("[REDACTED]");
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEntityModified_ShouldCreateUpdateAuditLogWithDiff()
        {
            // Arrange
            var (context, _) = CreateContextWithInterceptor(userId: "user-editor-1");

            var genero = new Genero { TituloGenero = "RPG" };
            context.Generos.Add(genero);
            await context.SaveChangesAsync();

            context.AuditLogs.RemoveRange(context.AuditLogs);
            await context.SaveChangesAsync();

            // Act
            genero.TituloGenero = "RPG de Ação";
            await context.SaveChangesAsync();

            // Assert
            var auditLogs = await context.AuditLogs.ToListAsync();
            auditLogs.Should().HaveCount(1);

            var audit = auditLogs.First();
            audit.Entidade.Should().Be(nameof(Genero));
            audit.EntidadeId.Should().Be(genero.Id.ToString());
            audit.TipoAcao.Should().Be("UPDATE");
            audit.UsuarioId.Should().Be("user-editor-1");

            var antigos = JsonSerializer.Deserialize<Dictionary<string, object?>>(audit.ValoresAntigos!);
            var novos = JsonSerializer.Deserialize<Dictionary<string, object?>>(audit.ValoresNovos!);

            antigos.Should().ContainKey("TituloGenero");
            antigos!["TituloGenero"]?.ToString().Should().Be("RPG");

            novos.Should().ContainKey("TituloGenero");
            novos!["TituloGenero"]?.ToString().Should().Be("RPG de Ação");
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEntityDeleted_ShouldCreateDeleteAuditLog()
        {
            // Arrange
            var (context, _) = CreateContextWithInterceptor(userId: "admin-deleter");

            var empresa = new Empresa { NomeEmpresa = "Bethesda" };
            context.Empresa.Add(empresa);
            await context.SaveChangesAsync();

            context.AuditLogs.RemoveRange(context.AuditLogs);
            await context.SaveChangesAsync();

            // Act
            context.Empresa.Remove(empresa);
            await context.SaveChangesAsync();

            // Assert
            var auditLogs = await context.AuditLogs.ToListAsync();
            auditLogs.Should().HaveCount(1);

            var audit = auditLogs.First();
            audit.Entidade.Should().Be(nameof(Empresa));
            audit.EntidadeId.Should().Be(empresa.Id.ToString());
            audit.TipoAcao.Should().Be("DELETE");
            audit.ValoresNovos.Should().BeNull();
            audit.ValoresAntigos.Should().NotBeNull();

            var antigos = JsonSerializer.Deserialize<Dictionary<string, object?>>(audit.ValoresAntigos!);
            antigos.Should().ContainKey("NomeEmpresa");
            antigos!["NomeEmpresa"]?.ToString().Should().Be("Bethesda");
        }

        [Fact]
        public async Task SaveChangesAsync_WhenSuppressAuditLoggingIsTrue_ShouldNotCreateAuditLog()
        {
            // Arrange
            var (context, _) = CreateContextWithInterceptor();
            context.SuppressAuditLogging = true;

            var genero = new Genero { TituloGenero = "Estratégia" };
            context.Generos.Add(genero);

            // Act
            await context.SaveChangesAsync();

            // Assert
            var auditLogs = await context.AuditLogs.ToListAsync();
            auditLogs.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChangesAsync_WhenSavingAuditLogDirectly_ShouldNotRecursivelyAuditItself()
        {
            // Arrange
            var (context, _) = CreateContextWithInterceptor();

            var directAudit = new AuditLog
            {
                Entidade = "ManualEntry",
                EntidadeId = Guid.NewGuid().ToString(),
                TipoAcao = "SYSTEM",
                TimestampUtc = DateTime.UtcNow
            };

            // Act
            context.AuditLogs.Add(directAudit);
            await context.SaveChangesAsync();

            // Assert
            var auditLogs = await context.AuditLogs.ToListAsync();
            auditLogs.Should().HaveCount(1);
            auditLogs.First().Entidade.Should().Be("ManualEntry");
        }
    }
}
