using System;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.BackgroundServices
{
    public class AuditRetentionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditRetentionBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _checkInterval;

        public AuditRetentionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AuditRetentionBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _checkInterval = TimeSpan.FromHours(24);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[AuditRetentionBackgroundService] Serviço de retenção de auditoria iniciado. Intervalo de execução: 24h.");

            // Pequeno delay inicial para não competir com a inicialização e seeder
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var retentionDays = _configuration.GetValue<int>("AuditLog:RetentionDays", 90);
                    if (int.TryParse(Environment.GetEnvironmentVariable("AUDIT_RETENTION_DAYS"), out var envRetentionDays) && envRetentionDays > 0)
                    {
                        retentionDays = envRetentionDays;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var retentionService = scope.ServiceProvider.GetRequiredService<IAuditRetentionService>();
                        var deleted = await retentionService.ExpurgaLogsAntigosAsync(retentionDays, stoppingToken);
                        _logger.LogInformation("[AuditRetentionBackgroundService] Execução periódica finalizada. {DeletedCount} registros expurgados.", deleted);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AuditRetentionBackgroundService] Erro durante o expurgo periódico de logs de auditoria.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("[AuditRetentionBackgroundService] Serviço de retenção de auditoria finalizado.");
        }
    }
}
