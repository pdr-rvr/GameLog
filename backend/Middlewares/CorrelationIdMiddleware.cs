using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace GameLog_Backend.Middlewares
{
    public class CorrelationIdMiddleware
    {
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var rawCorrelationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

            var correlationId = IsValidCorrelationId(rawCorrelationId)
                ? rawCorrelationId!
                : Guid.NewGuid().ToString("D");

            context.Items[CorrelationIdHeaderName] = correlationId;

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers[CorrelationIdHeaderName] = correlationId;
                }
                return Task.CompletedTask;
            });

            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("UserId", userId))
            {
                await _next(context);
            }
        }

        private static bool IsValidCorrelationId(string? correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > 64)
                return false;

            foreach (var c in correlationId)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    return false;
            }

            return true;
        }
    }
}
