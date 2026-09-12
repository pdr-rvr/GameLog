using FluentAssertions;
using GameLog_Backend.Services.Catalog;
using Xunit;

namespace GameLog.Tests.Unit.Catalog
{
    public class CatalogSanitizerTests
    {
        [Theory]
        [InlineData("Cyberpunk 2077", true)]
        [InlineData("The Witcher 3: Wild Hunt", true)]
        [InlineData("Elden Ring", true)]
        [InlineData("Soundtrack Edition", false)]
        [InlineData("Game OST", false)]
        [InlineData("Expansion Pack DLC", false)]
        [InlineData("Wallpapers Pack", false)]
        [InlineData("Trainer / Cheat Engine", false)]
        public void EhJogoValido_DeveIdentificarJogosReaisEFiltrarConteudoIrrelevante(string titulo, bool esperado)
        {
            // Act
            var result = CatalogSanitizer.EhJogoValido(titulo);

            // Assert
            result.Should().Be(esperado);
        }

        [Theory]
        [InlineData("Cyberpunk 2077 (2020)", "cyberpunk2077")]
        [InlineData("The Witcher 3: Wild Hunt - Game of the Year Edition", "thewitcher3wildhunt")]
        [InlineData("Dark Souls: Remastered", "darksoulsremastered")]
        [InlineData("Persona 5: Royal (Special Edition)", "persona5royal")]
        public void NormalizarTituloParaDeduplicacao_DeveRemoverSufixosDeEdicoesEAno(string titulo, string esperado)
        {
            // Act
            var result = CatalogSanitizer.NormalizarTituloParaDeduplicacao(titulo);

            // Assert
            result.Should().Be(esperado);
        }

        [Fact]
        public void SanitizarTextoDescricaoHtml_DeveRemoverTagsHtmlEEntidades()
        {
            // Arrange
            var html = "<p>Bem-vindo ao <strong>mundo de fantasia</strong>! &amp; desvende segredos.<br />Boa sorte!</p>";

            // Act
            var result = CatalogSanitizer.SanitizarTextoDescricaoHtml(html);

            // Assert
            result.Should().NotContain("<p>");
            result.Should().NotContain("<strong>");
            result.Should().NotContain("&amp;");
            result.Should().Contain("&");
            result.Should().Contain("mundo de fantasia");
        }

        [Theory]
        [InlineData("https://store.steampowered.com/app/1091500/Cyberpunk_2077/", "1091500")]
        [InlineData("https://store.steampowered.com/app/292030/", "292030")]
        [InlineData("https://store.playstation.com/product/1234", null)]
        [InlineData(null, null)]
        public void ExtrairSteamAppId_DeveExtrairAppIdCorreto(string? url, string? esperado)
        {
            // Act
            var result = CatalogSanitizer.ExtrairSteamAppId(url);

            // Assert
            result.Should().Be(esperado);
        }

        [Fact]
        public void ResolverMelhorCapaHd_ComLojaSteam_DeveRetornarCapaVerticalHdSteam()
        {
            // Arrange
            var stores = new List<GameLog_Backend.DTOs.RawgStoreItemDTO>
            {
                new()
                {
                    Url = "https://store.steampowered.com/app/1091500/Cyberpunk_2077/",
                    Store = new GameLog_Backend.DTOs.RawgNamedEntityDTO { Slug = "steam" }
                }
            };

            // Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd("https://media.rawg.io/media/crop/600/400/games/123.jpg", stores);

            // Assert
            result.Should().Be("https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1091500/library_600x900_2x.jpg");
        }

        [Fact]
        public void ResolverMelhorCapaHd_SemSteam_DeveSanitizarUrlRawgParaOriginal()
        {
            // Arrange
            var rawgCrop = "https://media.rawg.io/media/crop/600/400/games/abc/def.jpg";

            // Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd(rawgCrop, null);

            // Assert
            result.Should().Be("https://media.rawg.io/media/games/abc/def.jpg");
        }
    }
}
