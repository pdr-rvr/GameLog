using System;
using FluentAssertions;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class RawgApiServiceTests
    {
        [Theory]
        [InlineData("The Witcher 3: Wild Hunt", true)]
        [InlineData("Elden Ring", true)]
        [InlineData("God of War Ragnarök", true)]
        [InlineData("The Witcher 3: Wild Hunt - Blood and Wine", false)] // DLC
        [InlineData("Cyberpunk 2077: Phantom Liberty", false)] // DLC
        [InlineData("Dark Souls - Daughters of Ash", false)] // Mod / Fangame
        [InlineData("Super Mario Demo", false)] // Demo
        [InlineData("Resident Evil 4 Deluxe Edition", false)] // Edition
        [InlineData("Street Fighter Soundtrack", false)] // Soundtrack
        public void EhJogoValido_ValidacaoNome_DeveFiltrarCorretamente(string nome, bool esperado)
        {
            var resultado = RawgApiService.EhJogoValido(nome);
            resultado.Should().Be(esperado);
        }

        [Fact]
        public void EhJogoValido_ItemComParentsCountMaiorQueZero_DeveRetornarFalso()
        {
            var item = new RawgGameItemDTO
            {
                Name = "Elden Ring: Shadow of the Erdtree",
                ParentsCount = 1,
                Added = 500,
                RatingsCount = 100
            };

            var resultado = RawgApiService.EhJogoValido(item);
            resultado.Should().BeFalse();
        }

        [Fact]
        public void EhJogoValido_ItemValidoComRelevancia_DeveRetornarTrue()
        {
            var item = new RawgGameItemDTO
            {
                Name = "Grand Theft Auto VI",
                ParentsCount = 0,
                Added = 1000,
                RatingsCount = 50
            };

            var resultado = RawgApiService.EhJogoValido(item);
            resultado.Should().BeTrue();
        }

        [Theory]
        [InlineData("action", "Ação")]
        [InlineData("role-playing-games-rpg", "RPG")]
        [InlineData("adventure", "Aventura")]
        [InlineData("shooter", "Tiro (FPS / TPS)")]
        [InlineData("strategy", "Estratégia")]
        [InlineData("racing", "Corrida")]
        [InlineData("sports", "Esportes")]
        [InlineData("fighting", "Luta")]
        [InlineData("unknown-genre", "Unknown-genre")]
        public void MapearGeneroParaPortugues_DeveMapearCorretamente(string generoRaw, string esperado)
        {
            var resultado = RawgApiService.MapearGeneroParaPortugues(generoRaw);
            resultado.Should().Be(esperado);
        }

        [Theory]
        [InlineData("mature", 18)]
        [InlineData("adults-only", 18)]
        [InlineData("teen", 14)]
        [InlineData("everyone-10-plus", 10)]
        [InlineData("everyone", 0)]
        [InlineData(null, 0)]
        public void MapearEsrbParaClassificacao_DeveRetornarIdadeCorreta(string? esrb, int esperado)
        {
            var resultado = RawgApiService.MapearEsrbParaClassificacao(esrb);
            resultado.Should().Be(esperado);
        }

        [Theory]
        [InlineData("The Legend of Zelda: Tears of the Kingdom", "Nintendo")]
        [InlineData("Super Mario Odyssey", "Nintendo")]
        [InlineData("God of War Ragnarok", "PlayStation Studios")]
        [InlineData("Halo Infinite", "Xbox Game Studios")]
        [InlineData("Grand Theft Auto V", "Rockstar Games")]
        [InlineData("The Witcher 3: Wild Hunt", "CD Projekt Red")]
        [InlineData("Resident Evil 4 Remake", "Capcom")]
        [InlineData("Final Fantasy VII Rebirth", "Square Enix")]
        [InlineData("Elden Ring", "FromSoftware")]
        [InlineData("Desconhecido Jogo Indie", null)]
        public void ResolverEmpresaPorFranquia_DeveIdentificarEstudio(string titulo, string? esperado)
        {
            var resultado = RawgApiService.ResolverEmpresaPorFranquia(titulo);
            resultado.Should().Be(esperado);
        }
    }
}
