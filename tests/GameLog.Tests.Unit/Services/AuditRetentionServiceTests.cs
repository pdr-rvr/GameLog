using System;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class AuditRetentionServiceTests
    {
        [Fact]
        public async Task ExpurgaLogsAntigosAsync_DeveRemoverLogsAlemDoPeriodoDeRetencao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var logger = NullLogger<AuditRetentionService>.Instance;
            var service = new AuditRetentionService(context, logger);

            var logAntigo = new AuditLog
            {
                Entidade = "Jogo",
                EntidadeId = Guid.NewGuid().ToString(),
                TipoAcao = "INSERT",
                TimestampUtc = DateTime.UtcNow.AddDays(-100) // 100 dias atras (> 90)
            };

            var logRecente = new AuditLog
            {
                Entidade = "Jogo",
                EntidadeId = Guid.NewGuid().ToString(),
                TipoAcao = "INSERT",
                TimestampUtc = DateTime.UtcNow.AddDays(-10) // 10 dias atras (< 90)
            };

            context.AuditLogs.AddRange(logAntigo, logRecente);
            await context.SaveChangesAsync();

            // Act
            var removidos = await service.ExpurgaLogsAntigosAsync(diasRetencao: 90);

            // Assert
            removidos.Should().Be(1);
            var logsRestantes = await context.AuditLogs.ToListAsync();
            logsRestantes.Should().HaveCount(1);
            logsRestantes[0].Id.Should().Be(logRecente.Id);
        }

        [Fact]
        public async Task ExpurgaLogsAntigosAsync_QuandoNenhumLogExcederLimite_DeveRetornarZero()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var logger = NullLogger<AuditRetentionService>.Instance;
            var service = new AuditRetentionService(context, logger);

            var logRecente = new AuditLog
            {
                Entidade = "Jogo",
                EntidadeId = Guid.NewGuid().ToString(),
                TipoAcao = "UPDATE",
                TimestampUtc = DateTime.UtcNow.AddDays(-5)
            };

            context.AuditLogs.Add(logRecente);
            await context.SaveChangesAsync();

            // Act
            var removidos = await service.ExpurgaLogsAntigosAsync(diasRetencao: 90);

            // Assert
            removidos.Should().Be(0);
            var logsRestantes = await context.AuditLogs.ToListAsync();
            logsRestantes.Should().HaveCount(1);
        }

        [Fact]
        public async Task ExpurgaLogsAntigosAsync_ComPeriodoInvalido_DeveLancarExcecao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var logger = NullLogger<AuditRetentionService>.Instance;
            var service = new AuditRetentionService(context, logger);

            // Act
            var act = () => service.ExpurgaLogsAntigosAsync(diasRetencao: 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
