using System.Text;
using System.Text.Json;
using AutoMapper;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.Interceptors;
using GameLog_Backend.Middlewares;
using GameLog_Backend.Profiles;
using GameLog_Backend.Seeders;
using GameLog_Backend.Services;
using GameLog_Backend.Services.Catalog;
using GameLog_Backend.Services.Interfaces;
using GameLog_Backend.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext();

if (builder.Environment.IsDevelopment())
{
    loggerConfig.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
}
else
{
    loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());
}

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();

if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.TraversePath().Load();
}

var isTesting = builder.Environment.IsEnvironment("Testing");

var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "gamelog";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

if (!isTesting)
{
    if (string.IsNullOrWhiteSpace(dbPassword))
    {
        throw new InvalidOperationException(
            "FATAL: A variável de ambiente 'DB_PASSWORD' não foi configurada. Defina-a no arquivo .env ou nas variáveis do ambiente.");
    }

    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    {
        throw new InvalidOperationException(
            "FATAL: A variável de ambiente 'JWT_SECRET' é obrigatória e deve possuir no mínimo 32 caracteres (256 bits) para garantir a segurança do algoritmo HMAC-SHA256.");
    }
}
else
{
    dbPassword ??= "TestingDbPassword";
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    {
        jwtSecret = "TestingSecretKeyMustBeAtLeast32CharactersLong123456!";
    }
}

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host={DB_SERVER};Port={DB_PORT};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD};";

var completeConnectionString = baseConnectionString
    .Replace("{DB_SERVER}", dbServer)
    .Replace("{DB_PORT}", dbPort)
    .Replace("{DB_NAME}", dbName)
    .Replace("{DB_USER}", dbUser)
    .Replace("{DB_PASSWORD}", dbPassword);

Console.WriteLine($"[GameLog] Conectando ao PostgreSQL em: {dbServer}:{dbPort}, Database: {dbName}");

var allowedOriginsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
var allowedOrigins = !string.IsNullOrWhiteSpace(allowedOriginsEnv)
    ? allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : new[] { "http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000", "http://127.0.0.1:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("ExternalApiLimiter", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => JsonNamingPolicy.CamelCase.ConvertName(kvp.Key),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Erro de Validação",
                status = StatusCodes.Status400BadRequest,
                detail = "Um ou mais campos contêm erros de validação.",
                instance = context.HttpContext.Request.Path.Value,
                traceId = context.HttpContext.TraceIdentifier,
                errors
            };

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CriarUsuarioDTOValidator>();

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtSecret;
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "GameLogAPI";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "GameLogClient";
    options.ExpireMinutes = 15;
    options.RefreshTokenExpireDays = 7;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GameLogAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GameLogClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<GameLogContext>((sp, options) =>
{
    options.UseNpgsql(completeConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IRawgApiService, RawgApiService>()
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 5;
    });

builder.Services.AddHttpClient<SteamGridDbService>()
    .AddStandardResilienceHandler(options =>
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 5;
    });
builder.Services.AddScoped<MassiveCatalogSeeder>();

builder.Services.AddAutoMapper(typeof(UsuarioProfile).Assembly);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICacheService, DistributedCacheService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUsuarioService>(sp => new UsuarioServices(
    sp.GetRequiredService<IUserProfileService>(),
    sp.GetRequiredService<IAuthService>(),
    sp.GetRequiredService<ISocialService>(),
    sp.GetRequiredService<IFeedService>()));
builder.Services.AddScoped<IJogoService, JogoServices>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoServices>();
builder.Services.AddScoped<IEmpresaService, EmpresaServices>();
builder.Services.AddScoped<IBibliotecaService, BibliotecaServices>();
builder.Services.AddScoped<IListaService, ListaServices>();
builder.Services.AddScoped<IBuscaGlobalService, BuscaGlobalService>();
builder.Services.AddScoped<IComunidadeService, ComunidadeServices>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<GameLogContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("AllowReactApp");
app.UseRateLimiter();

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        var maxRetries = 15;
        var delaySeconds = 2;
        var connected = false;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var context = services.GetRequiredService<GameLogContext>();
                Console.WriteLine($"[GameLog] Tentativa {attempt}/{maxRetries} - Conectando ao PostgreSQL e garantindo schema...");

                context.Database.Migrate();

                var forceReseed = string.Equals(Environment.GetEnvironmentVariable("FORCE_RESEED"), "true", StringComparison.OrdinalIgnoreCase);
                if (app.Environment.IsDevelopment() || forceReseed)
                {
                    Console.WriteLine("[GameLog] Verificando catálogo e integridade com a RAWG...");
                    var massiveSeeder = services.GetRequiredService<MassiveCatalogSeeder>();
                    massiveSeeder.CleanAndSeedRealGamesAsync().GetAwaiter().GetResult();
                    Console.WriteLine("[GameLog] Povoamento/verificação de jogos finalizado com sucesso!");
                }
                else
                {
                    logger.LogInformation("[GameLog] Ambiente de Produção detectado sem FORCE_RESEED. Rotina destrutiva do seeder pulada para proteção de dados.");
                }

                connected = true;
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"[GameLog] PostgreSQL ainda não disponível (tentativa {attempt}/{maxRetries}): {ex.Message}");
                if (attempt < maxRetries)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }

        if (!connected)
        {
            logger.LogError("[GameLog] Não foi possível conectar ao PostgreSQL após múltiplas tentativas.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API GameLog está online!").AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

