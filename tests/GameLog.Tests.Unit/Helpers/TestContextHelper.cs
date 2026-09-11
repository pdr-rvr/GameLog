using AutoMapper;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GameLog.Tests.Unit.Helpers
{
    public static class TestContextHelper
    {
        public static GameLogContext CreateInMemoryContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<GameLogContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new GameLogContext(options);
        }

        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UsuarioProfile>();
                cfg.AddProfile<AvaliacaoProfile>();
                cfg.AddProfile<EmpresaProfile>();
            });

            return configuration.CreateMapper();
        }

        public static IOptions<JwtSettings> CreateJwtSettings()
        {
            return Options.Create(new JwtSettings
            {
                Key = "GameLogSuperSecretKeyForUnitTestingPurposesOnly123456!",
                Issuer = "GameLogTestAPI",
                Audience = "GameLogTestClient",
                ExpireHours = 24
            });
        }
    }
}
