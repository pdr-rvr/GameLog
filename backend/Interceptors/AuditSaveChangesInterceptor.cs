using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.Entities;
using GameLog_Backend.Helpers;
using GameLog_Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GameLog_Backend.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Senha",
            "SenhaHash",
            "Password",
            "TokenHash",
            "RefreshToken"
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false
        };

        public AuditSaveChangesInterceptor(IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                CreateAuditEntries(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                CreateAuditEntries(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CreateAuditEntries(DbContext context)
        {
            if (context is GameLogContext gameLogContext && gameLogContext.SuppressAuditLogging)
            {
                return;
            }

            var httpContext = _httpContextAccessor?.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var correlationId = httpContext?.Items[CorrelationIdMiddleware.CorrelationIdHeaderName] as string
                ?? httpContext?.TraceIdentifier;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            if (entries.Count == 0)
            {
                return;
            }

            var auditLogs = new List<AuditLog>();
            var timestamp = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var auditLog = CreateAuditLog(entry, userId, ipAddress, correlationId, timestamp);
                if (auditLog != null)
                {
                    auditLogs.Add(auditLog);
                }
            }

            if (auditLogs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditLogs);
            }
        }

        private static AuditLog? CreateAuditLog(
            EntityEntry entry,
            string? userId,
            string? ipAddress,
            string? correlationId,
            DateTime timestamp)
        {
            var entityName = entry.Metadata.ClrType.Name;
            var primaryKeyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = primaryKeyProperty?.CurrentValue?.ToString()
                ?? primaryKeyProperty?.OriginalValue?.ToString()
                ?? string.Empty;

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            string actionType;

            switch (entry.State)
            {
                case EntityState.Added:
                    actionType = "INSERT";
                    foreach (var prop in entry.Properties)
                    {
                        var propName = prop.Metadata.Name;
                        var val = SensitivePropertyNames.Contains(propName)
                            ? "[REDACTED]"
                            : prop.CurrentValue;
                        newValues[propName] = val;
                    }

                    if (string.IsNullOrEmpty(entityId) || entityId == Guid.Empty.ToString())
                    {
                        if (primaryKeyProperty?.Metadata.ClrType == typeof(Guid))
                        {
                            var newGuid = UuidV7Helper.NewGuid();
                            primaryKeyProperty.CurrentValue = newGuid;
                            entityId = newGuid.ToString();
                            if (newValues.ContainsKey(primaryKeyProperty.Metadata.Name))
                            {
                                newValues[primaryKeyProperty.Metadata.Name] = newGuid;
                            }
                        }
                    }
                    break;

                case EntityState.Modified:
                    actionType = "UPDATE";
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            var propName = prop.Metadata.Name;
                            var oldVal = SensitivePropertyNames.Contains(propName)
                                ? "[REDACTED]"
                                : prop.OriginalValue;
                            var newVal = SensitivePropertyNames.Contains(propName)
                                ? "[REDACTED]"
                                : prop.CurrentValue;

                            oldValues[propName] = oldVal;
                            newValues[propName] = newVal;
                        }
                    }

                    if (oldValues.Count == 0 && newValues.Count == 0)
                    {
                        return null;
                    }
                    break;

                case EntityState.Deleted:
                    actionType = "DELETE";
                    foreach (var prop in entry.Properties)
                    {
                        var propName = prop.Metadata.Name;
                        var val = SensitivePropertyNames.Contains(propName)
                            ? "[REDACTED]"
                            : prop.OriginalValue;
                        oldValues[propName] = val;
                    }
                    break;

                default:
                    return null;
            }

            return new AuditLog
            {
                Id = UuidV7Helper.NewGuid(),
                UsuarioId = userId,
                Entidade = entityName,
                EntidadeId = entityId,
                TipoAcao = actionType,
                ValoresAntigos = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues, JsonOptions) : null,
                ValoresNovos = newValues.Count > 0 ? JsonSerializer.Serialize(newValues, JsonOptions) : null,
                TimestampUtc = timestamp,
                IpAddress = ipAddress,
                CorrelationId = correlationId
            };
        }
    }
}
