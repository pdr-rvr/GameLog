using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.Services
{
    public class AuditRetentionService : IAuditRetentionService
    {
        private readonly GameLogContext _context;
        private readonly ILogger<AuditRetentionService> _logger;

        public AuditRetentionService(GameLogContext context, ILogger<AuditRetentionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> ExpurgaLogsAntigosAsync(int diasRetencao = 90, CancellationToken cancellationToken = default)
        {
            if (diasRetencao <= 0)
            {
                throw new ArgumentException("O período de retenção deve ser de pelo menos 1 dia.", nameof(diasRetencao));
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-diasRetencao);

            _logger.LogInformation("[AuditRetention] Iniciando expurgo de registros de auditoria anteriores a {CutoffDate:yyyy-MM-dd HH:mm:ss} UTC (retenção: {DiasRetencao} dias)", cutoffDate, diasRetencao);

            _context.SuppressAuditLogging = true;

            try
            {
                var logsParaDeletar = await _context.AuditLogs
                    .Where(a => a.TimestampUtc < cutoffDate)
                    .ToListAsync(cancellationToken);

                if (logsParaDeletar.Count > 0)
                {
                    _context.AuditLogs.RemoveRange(logsParaDeletar);
                    var deletedCount = await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("[AuditRetention] Expurgo concluído com sucesso. {DeletedCount} registros de auditoria foram removidos.", deletedCount);
                    return deletedCount;
                }

                _logger.LogInformation("[AuditRetention] Nenhum registro de auditoria anterior a {CutoffDate} foi encontrado para remoção.", cutoffDate);
                return 0;
            }
            finally
            {
                _context.SuppressAuditLogging = false;
            }
        }
    }
}
