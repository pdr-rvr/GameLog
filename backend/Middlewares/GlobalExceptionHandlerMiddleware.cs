using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GameLog Exception] Erro não tratado na rota {Path} [{Method}]: {Message}",
                    context.Request.Path,
                    context.Request.Method,
                    ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var (status, title, type) = exception switch
            {
                KeyNotFoundException => (
                    (int)HttpStatusCode.NotFound,
                    "Recurso Não Encontrado",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                ),
                UnauthorizedAccessException => (
                    (int)HttpStatusCode.Unauthorized,
                    "Não Autorizado",
                    "https://tools.ietf.org/html/rfc7235#section-3.1"
                ),
                ArgumentException or InvalidOperationException => (
                    (int)HttpStatusCode.BadRequest,
                    "Requisição Inválida",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                ),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    "Erro Interno no Servidor",
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                )
            };

            context.Response.StatusCode = status;

            // Para erros 500 em produção, ocultamos detalhes de banco / stack traces
            string detail;
            if (status == (int)HttpStatusCode.InternalServerError)
            {
                detail = _env.IsDevelopment()
                    ? exception.Message
                    : "Ocorreu um erro interno ao processar a sua requisição. Por favor, tente novamente mais tarde.";
            }
            else
            {
                detail = exception.Message;
            }

            var problemDetails = new
            {
                type,
                title,
                status,
                detail,
                instance = context.Request.Path.Value,
                traceId = context.TraceIdentifier
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json);
        }
    }
}
