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

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var completeConnectionString = baseConnectionString
    .Replace("{DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER"))
    .Replace("{DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
    .Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER"))
    .Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD"));

Console.WriteLine("Conectando ao banco em: " + Environment.GetEnvironmentVariable("DB_SERVER"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

var jwtConfig = builder.Configuration.GetSection("Jwt");
jwtConfig["Key"] = Environment.GetEnvironmentVariable("JWT_SECRET");

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


builder.Services.AddScoped<EmpresaSeeder>();

builder.Services.AddDbContext<GameLogContext>(options =>
    options.UseSqlServer(completeConnectionString));

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

    try
    {
        var context = services.GetRequiredService<GameLogContext>();

        Console.WriteLine("Iniciando migrations...");
        context.Database.Migrate();
        Console.WriteLine("Migrations aplicadas.");

        Console.WriteLine("Executando seeders...");
        new EmpresaSeeder(context).Seed();
        new GeneroSeeder(context).Seed();
        new JogoSeeder(context).Seed();
        new JogoGeneroSeeder(context).Seed();
        Console.WriteLine("Seeders executados com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao executar os seeders");
    }
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API GameLog está online!").AllowAnonymous();

app.Run();
