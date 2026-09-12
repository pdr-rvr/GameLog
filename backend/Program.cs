using System.Text;
using System.Text.Json;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.Middlewares;
using GameLog_Backend.Profiles;
using GameLog_Backend.Seeders;
using GameLog_Backend.Services;
using GameLog_Backend.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "gamelog";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "GameLog123!@#";
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "GameLogSuperSecretKeyDefault1234567890!";

// Garantir tamanho mínimo de 256 bits (32 bytes) para algoritmo HMAC-SHA256
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    jwtSecret = "GameLogSuperSecretKeyDefault1234567890!SecureLongKey256Bit";
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        corsBuilder => corsBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
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
    options.ExpireHours = 24;
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

builder.Services.AddDbContext<GameLogContext>(options =>
    options.UseNpgsql(completeConnectionString));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<RawgApiService>();
builder.Services.AddScoped<RawgApiService>();
builder.Services.AddScoped<MassiveCatalogSeeder>();

builder.Services.AddScoped<JogoServices>();
builder.Services.AddAutoMapper(typeof(UsuarioProfile));
builder.Services.AddScoped<UsuarioServices>();
builder.Services.AddAutoMapper(typeof(AvaliacaoProfile));
builder.Services.AddScoped<AvaliacaoServices>();
builder.Services.AddAutoMapper(typeof(EmpresaProfile));
builder.Services.AddScoped<EmpresaServices>();
builder.Services.AddScoped<BibliotecaServices>();
builder.Services.AddScoped<ListaServices>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<GameLogContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("AllowReactApp");

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

                context.Database.EnsureCreated();

                Console.WriteLine("[GameLog] Executando limpeza e povoamento do catálogo com jogos reais da RAWG...");
                var massiveSeeder = services.GetRequiredService<MassiveCatalogSeeder>();
                massiveSeeder.CleanAndSeedRealGamesAsync().GetAwaiter().GetResult();
                Console.WriteLine("[GameLog] Povoamento com jogos reais finalizado com sucesso!");

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API GameLog está online!").AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program { }

