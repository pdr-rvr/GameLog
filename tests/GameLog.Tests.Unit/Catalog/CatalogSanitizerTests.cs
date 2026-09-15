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

        [Fact]
        public void ResolverMelhorCapaHd_GrandTheftAutoVI_NaoDeveConfundirComGtaV()
        {
            // Act
            var resultGta6 = CatalogSanitizer.ResolverMelhorCapaHd(
                "https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg",
                stores: null,
                titulo: "Grand Theft Auto VI");

            var resultGta5 = CatalogSanitizer.ResolverMelhorCapaHd(
                "https://media.rawg.io/media/games/123/original.jpg",
                stores: null,
                titulo: "Grand Theft Auto V");

            // Assert
            resultGta6.Should().Be("https://media.rawg.io/media/games/734/7342a1cd82c8997ec620084ae4c2e7e4.jpg");
            resultGta5.Should().Be("https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/271590/library_600x900_2x.jpg");
        }

        [Theory]
        [InlineData("Pid", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/227860/library_600x900_2x.jpg")]
        [InlineData("SiN", "https://media.rawg.io/media/screenshots/559/559f0bc2b44bc3223f14e4393a2f70d8.jpg")]
        [InlineData("FINAL FANTASY VI", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1173820/library_600x900_2x.jpg")]
        [InlineData("Final Fantasy VII", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/39140/library_600x900_2x.jpg")]
        [InlineData("Elden Ring: Nightreign", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2622380/library_600x900_2x.jpg")]
        [InlineData("Portal", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/400/library_600x900_2x.jpg")]
        [InlineData("Portal 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/620/library_600x900_2x.jpg")]
        [InlineData("Fallout", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/38400/library_600x900_2x.jpg")]
        [InlineData("Fallout 4", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/377160/library_600x900_2x.jpg")]
        [InlineData("Red Dead Redemption", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2668510/library_600x900_2x.jpg")]
        [InlineData("Red Dead Redemption 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1174180/library_600x900_2x.jpg")]
        [InlineData("Max Payne 3", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/204100/library_600x900_2x.jpg")]
        public void ResolverMelhorCapaHd_SequenciasEColisoes_DevemResolverCapasCorretas(string titulo, string capaEsperada)
        {
            // Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd(
                "https://media.rawg.io/media/games/default.jpg",
                stores: null,
                titulo: titulo);

            // Assert
            result.Should().Be(capaEsperada);
        }
    }
}
