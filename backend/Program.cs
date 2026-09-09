using System.Text;
using DotNetEnv;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.Middlewares;
using GameLog_Backend.Profiles;
using GameLog_Backend.Seeders;
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

                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[CurtidasDeAvaliacoes]') 
                        AND name = 'UsuarioId'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[CurtidasDeAvaliacoes] ADD [UsuarioId] INT NULL;
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[RespostasDeAvaliacao]') 
                        AND name = 'UsuarioId'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[RespostasDeAvaliacao] ADD [UsuarioId] INT NULL;
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[RespostasDeAvaliacao]') 
                        AND name = 'DataCriacao'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[RespostasDeAvaliacao] ADD [DataCriacao] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
                    END

                    IF EXISTS (
                        SELECT * FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[RespostasDeAvaliacao]') 
                        AND name = 'Comentario'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[RespostasDeAvaliacao] ALTER COLUMN [Comentario] NVARCHAR(500) NOT NULL;
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.tables 
                        WHERE name = 'CurtidasDeRespostas'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[CurtidasDeRespostas] (
                            [CurtidaDeRespostaId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Curtida] BIT NOT NULL,
                            [RespostaDeAvaliacaoId] INT NOT NULL,
                            [UsuarioId] INT NOT NULL,
                            [EstaAtivo] BIT NOT NULL,
                            CONSTRAINT [FK_CurtidasDeRespostas_Respostas] FOREIGN KEY ([RespostaDeAvaliacaoId]) REFERENCES [dbo].[RespostasDeAvaliacao]([RespostaDeAvaliacaoId]) ON DELETE CASCADE,
                            CONSTRAINT [FK_CurtidasDeRespostas_Usuarios] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios]([UsuarioId])
                        );
                        CREATE INDEX [IX_CurtidasDeRespostas_RespostaDeAvaliacaoId] ON [dbo].[CurtidasDeRespostas]([RespostaDeAvaliacaoId]);
                        CREATE INDEX [IX_CurtidasDeRespostas_UsuarioId] ON [dbo].[CurtidasDeRespostas]([UsuarioId]);
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.tables 
                        WHERE name = 'ItensBiblioteca'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[ItensBiblioteca] (
                            [BibliotecaJogoId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [UsuarioId] INT NOT NULL,
                            [JogoId] INT NOT NULL,
                            [Status] INT NOT NULL,
                            [DataAtualizacao] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [DataConclusao] DATETIME2 NULL,
                            [EstaAtivo] BIT NOT NULL DEFAULT 1,
                            CONSTRAINT [FK_ItensBiblioteca_Usuarios] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios]([UsuarioId]) ON DELETE CASCADE,
                            CONSTRAINT [FK_ItensBiblioteca_Jogos] FOREIGN KEY ([JogoId]) REFERENCES [dbo].[Jogos]([JogoId]) ON DELETE CASCADE
                        );
                        CREATE UNIQUE INDEX [IX_ItensBiblioteca_Usuario_Jogo] ON [dbo].[ItensBiblioteca]([UsuarioId], [JogoId]);
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.tables 
                        WHERE name = 'JogosFavoritosUsuarios'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[JogosFavoritosUsuarios] (
                            [JogoFavoritoUsuarioId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [UsuarioId] INT NOT NULL,
                            [JogoId] INT NOT NULL,
                            [Posicao] INT NOT NULL,
                            [EstaAtivo] BIT NOT NULL DEFAULT 1,
                            CONSTRAINT [FK_JogosFavoritos_Usuarios] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios]([UsuarioId]) ON DELETE CASCADE,
                            CONSTRAINT [FK_JogosFavoritos_Jogos] FOREIGN KEY ([JogoId]) REFERENCES [dbo].[Jogos]([JogoId]) ON DELETE CASCADE
                        );
                        CREATE UNIQUE INDEX [IX_JogosFavoritos_Usuario_Posicao] ON [dbo].[JogosFavoritosUsuarios]([UsuarioId], [Posicao]);
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.tables 
                        WHERE name = 'ListasDeJogos'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[ListasDeJogos] (
                            [ListaDeJogosId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [UsuarioId] INT NOT NULL,
                            [Titulo] NVARCHAR(100) NOT NULL,
                            [Descricao] NVARCHAR(500) NULL,
                            [EstaPublica] BIT NOT NULL DEFAULT 1,
                            [DataCriacao] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [DataAtualizacao] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [EstaAtivo] BIT NOT NULL DEFAULT 1,
                            CONSTRAINT [FK_ListasDeJogos_Usuarios] FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios]([UsuarioId]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_ListasDeJogos_UsuarioId] ON [dbo].[ListasDeJogos]([UsuarioId]);
                    END

                    IF NOT EXISTS (
                        SELECT * FROM sys.tables 
                        WHERE name = 'ItensDeListas'
                    )
                    BEGIN
                        CREATE TABLE [dbo].[ItensDeListas] (
                            [ItemDeListaId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [ListaDeJogosId] INT NOT NULL,
                            [JogoId] INT NOT NULL,
                            [Ordem] INT NOT NULL DEFAULT 1,
                            [DataAdicionado] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [EstaAtivo] BIT NOT NULL DEFAULT 1,
                            CONSTRAINT [FK_ItensDeListas_Listas] FOREIGN KEY ([ListaDeJogosId]) REFERENCES [dbo].[ListasDeJogos]([ListaDeJogosId]) ON DELETE CASCADE,
                            CONSTRAINT [FK_ItensDeListas_Jogos] FOREIGN KEY ([JogoId]) REFERENCES [dbo].[Jogos]([JogoId]) ON DELETE CASCADE
                        );
                        CREATE UNIQUE INDEX [IX_ItensDeListas_Lista_Jogo] ON [dbo].[ItensDeListas]([ListaDeJogosId], [JogoId]);
                    END

                    -- Ajuste de capacidade de colunas para catálogos ricos e RAWG
                    ALTER TABLE [dbo].[Jogos] ALTER COLUMN [Titulo] NVARCHAR(250) NOT NULL;
                    ALTER TABLE [dbo].[Jogos] ALTER COLUMN [Descricao] NVARCHAR(MAX) NULL;
                    ALTER TABLE [dbo].[Empresa] ALTER COLUMN [NomeEmpresa] NVARCHAR(150) NOT NULL;
                    ALTER TABLE [dbo].[Generos] ALTER COLUMN [TituloGenero] NVARCHAR(50) NOT NULL;
                ");
            }
            catch (Exception exCol)
            {
                Console.WriteLine($"[GameLog] Verificação de colunas complementares: {exCol.Message}");
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
