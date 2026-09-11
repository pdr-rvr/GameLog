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
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "GameLog";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "GameLog123!@#";
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "GameLogSuperSecretKeyDefault1234567890!";

// Garantir tamanho mínimo de 256 bits (32 bytes) para algoritmo HMAC-SHA256
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    jwtSecret = "GameLogSuperSecretKeyDefault1234567890!SecureLongKey256Bit";
}

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server={DB_SERVER};Database={DB_NAME};User ID={DB_USER};Password={DB_PASSWORD};TrustServerCertificate=True;";

var completeConnectionString = baseConnectionString
    .Replace("{DB_SERVER}", dbServer)
    .Replace("{DB_NAME}", dbName)
    .Replace("{DB_USER}", dbUser)
    .Replace("{DB_PASSWORD}", dbPassword);

Console.WriteLine($"[GameLog] Conectando ao banco em: {dbServer}, Database: {dbName}");

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
    options.UseSqlServer(completeConnectionString));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<RawgApiService>();
builder.Services.AddScoped<RawgApiService>();
builder.Services.AddScoped<MassiveCatalogSeeder>();

builder.Services.AddScoped<EmpresaSeeder>();
builder.Services.AddScoped<GeneroSeeder>();
builder.Services.AddScoped<JogoSeeder>();
builder.Services.AddScoped<JogoGeneroSeeder>();

builder.Services.AddScoped<JogoServices>();
builder.Services.AddAutoMapper(typeof(UsuarioProfile));
builder.Services.AddScoped<UsuarioServices>();
builder.Services.AddAutoMapper(typeof(AvaliacaoProfile));
builder.Services.AddScoped<AvaliacaoServices>();
builder.Services.AddAutoMapper(typeof(EmpresaProfile));
builder.Services.AddScoped<EmpresaServices>();
builder.Services.AddScoped<BibliotecaServices>();
builder.Services.AddScoped<ListaServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("AllowReactApp");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    var maxRetries = 15;
    var delaySeconds = 3;
    var connected = false;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var context = services.GetRequiredService<GameLogContext>();
            Console.WriteLine($"[GameLog] Tentativa {attempt}/{maxRetries} - Verificando conexão e inicializando banco com UUIDv7...");
            
            try
            {
                var isGuidSchema = false;
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(completeConnectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'UsuarioId' ELSE SELECT 'NONE'";
                        var dt = cmd.ExecuteScalar()?.ToString();
                        if (string.Equals(dt, "uniqueidentifier", StringComparison.OrdinalIgnoreCase))
                        {
                            isGuidSchema = true;
                        }
                    }
                }

                if (!isGuidSchema)
                {
                    Console.WriteLine("[GameLog] Detectado schema legado ou banco não inicializado. Recriando banco de dados com UUIDv7 (UNIQUEIDENTIFIER)...");
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
                else
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception exInit)
            {
                Console.WriteLine($"[GameLog] Inicializando schema via EnsureCreated: {exInit.Message}");
                context.Database.EnsureCreated();
            }

            Console.WriteLine("[GameLog] Executando limpeza e povoamento do catálogo com jogos 100% reais e oficiais da RAWG...");
            var massiveSeeder = services.GetRequiredService<MassiveCatalogSeeder>();
            massiveSeeder.CleanAndSeedRealGamesAsync().GetAwaiter().GetResult();
            Console.WriteLine("[GameLog] Povoamento com jogos reais finalizado com sucesso!");

            connected = true;
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"[GameLog] Banco de dados ainda não disponível (tentativa {attempt}/{maxRetries}): {ex.Message}");
            if (attempt < maxRetries)
            {
                Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }

    if (!connected)
    {
        logger.LogError("[GameLog] Não foi possível conectar ao banco de dados após múltiplas tentativas.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API GameLog está online!").AllowAnonymous();

app.Run();
