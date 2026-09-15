using System;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GameLog.Tests.Unit.Middlewares
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ComHeaderValido_DeveManterCorrelationId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var validId = "custom-correlation-123-abc";
            context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = validId;

            var nextExecuted = false;
            RequestDelegate next = (ctx) =>
            {
                nextExecuted = true;
                return Task.CompletedTask;
            };

            var middleware = new CorrelationIdMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextExecuted.Should().BeTrue();
            context.Items[CorrelationIdMiddleware.CorrelationIdHeaderName].Should().Be(validId);
        }

        [Fact]
        public async Task InvokeAsync_ComHeaderInvalidoOuTentativaDeLogInjection_DeveGerarNovoGuid()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var maliciousId = "invalid-id\n[CRITICAL] Injected Log Line\r\n";
            context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeaderName] = maliciousId;

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new CorrelationIdMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var generatedId = context.Items[CorrelationIdMiddleware.CorrelationIdHeaderName] as string;
            generatedId.Should().NotBeNullOrEmpty();
            generatedId.Should().NotBe(maliciousId);
            generatedId.Should().NotContain("\n");
            generatedId.Should().NotContain("\r");
        }

        [Fact]
        public async Task InvokeAsync_SemHeader_DeveGerarNovoGuid()
        {
            // Arrange
            var context = new DefaultHttpContext();
            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new CorrelationIdMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var generatedId = context.Items[CorrelationIdMiddleware.CorrelationIdHeaderName] as string;
            generatedId.Should().NotBeNullOrEmpty();
            Guid.TryParse(generatedId, out _).Should().BeTrue();
        }
    }
}
