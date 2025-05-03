using DotNetEnv;
using GameLog_Backend.Database;
using GameLog_Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


Env.Load();


var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var completeConnectionString = baseConnectionString
    .Replace("{DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER"))
    .Replace("{DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
    .Replace("{DB_USER}", Environment.GetEnvironmentVariable("DB_USER"))
    .Replace("{DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD"));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddScoped<EmpresaSeeder>();

builder.Services.AddDbContext<GameLogContext>(options =>
    options.UseSqlServer(completeConnectionString));

builder.Services.AddScoped<JogoServices>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<GameLogContext>();

        // Aplica as migrations automaticamente
        context.Database.Migrate();

        // Executa os seeders na ordem correta
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
