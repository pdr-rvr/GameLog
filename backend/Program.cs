using System.Text;
using DotNetEnv;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.Profiles;
using GameLog_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

builder.Services.AddControllers();

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

builder.Services.AddScoped<EmpresaSeeder>();
builder.Services.AddScoped<GeneroSeeder>();
builder.Services.AddScoped<JogoSeeder>();
builder.Services.AddScoped<JogoGeneroSeeder>();

builder.Services.AddScoped<JogoServices>();
builder.Services.AddAutoMapper(typeof(UsuarioProfile));
builder.Services.AddScoped<UsuarioServices>();
builder.Services.AddAutoMapper(typeof(AvaliacaoProfile));
builder.Services.AddScoped<AvaliacaoServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
            Console.WriteLine($"[GameLog] Tentativa {attempt}/{maxRetries} - Verificando conexão e aplicando Migrations...");
            context.Database.Migrate();
            Console.WriteLine("[GameLog] Migrations aplicadas com sucesso.");

            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') 
                        AND name = 'Bio'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Usuarios] ADD [Bio] NVARCHAR(300) NULL;
                    END
                ");
            }
            catch (Exception exCol)
            {
                Console.WriteLine($"[GameLog] Verificação de coluna Bio: {exCol.Message}");
            }

            Console.WriteLine("[GameLog] Executando Seeders...");
            new EmpresaSeeder(context).Seed();
            new GeneroSeeder(context).Seed();
            new JogoSeeder(context).Seed();
            new JogoGeneroSeeder(context).Seed();
            Console.WriteLine("[GameLog] Seeders executados com sucesso!");

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
