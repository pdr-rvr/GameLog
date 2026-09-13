using System;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog_Backend.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class DistributedCacheServiceTests
    {
        private DistributedCacheService CriarServico()
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            var distributedCache = new MemoryDistributedCache(options);
            return new DistributedCacheService(distributedCache);
        }

        private class ExemploCache
        {
            public string Nome { get; set; } = string.Empty;
            public int Valor { get; set; }
        }

        [Fact]
        public async Task GetAsync_ChaveInexistente_DeveRetornarDefault()
        {
            var service = CriarServico();
            var resultado = await service.GetAsync<ExemploCache>("chave_inexistente");
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task SetAsync_E_GetAsync_ComDadosValidos_DeveRecuperarObjeto()
        {
            var service = CriarServico();
            var objetoOriginal = new ExemploCache { Nome = "Zelda", Valor = 100 };

            await service.SetAsync("jogo_zelda", objetoOriginal, TimeSpan.FromMinutes(5));
            var resultado = await service.GetAsync<ExemploCache>("jogo_zelda");

            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("Zelda");
            resultado.Valor.Should().Be(100);
        }

        [Fact]
        public async Task RemoveAsync_DeveExcluirChaveDoCache()
        {
            var service = CriarServico();
            var objetoOriginal = new ExemploCache { Nome = "Mario", Valor = 90 };

            await service.SetAsync("jogo_mario", objetoOriginal, TimeSpan.FromMinutes(5));
            await service.RemoveAsync("jogo_mario");

            var resultado = await service.GetAsync<ExemploCache>("jogo_mario");
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetOrCreateAsync_QuandoNaoExiste_DeveInvocarFactoryESalvar()
        {
            var service = CriarServico();
            var chamadasFactory = 0;

            var resultado = await service.GetOrCreateAsync("jogo_metroid", async () =>
            {
                chamadasFactory++;
                await Task.Yield();
                return new ExemploCache { Nome = "Metroid", Valor = 95 };
            }, TimeSpan.FromMinutes(5));

            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Metroid");
            chamadasFactory.Should().Be(1);

            // Segunda chamada não deve invocar a factory
            var resultado2 = await service.GetOrCreateAsync("jogo_metroid", async () =>
            {
                chamadasFactory++;
                await Task.Yield();
                return new ExemploCache { Nome = "Metroid Repetido", Valor = 0 };
            }, TimeSpan.FromMinutes(5));

            resultado2.Nome.Should().Be("Metroid");
            chamadasFactory.Should().Be(1);
        }
    }
}
