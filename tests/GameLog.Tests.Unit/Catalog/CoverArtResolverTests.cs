using System.Collections.Generic;
using FluentAssertions;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services.Catalog;
using Xunit;

namespace GameLog.Tests.Unit.Catalog
{
    public class CoverArtResolverTests
    {
        [Fact]
        public void ResolverMelhorCapaHd_Camada1_SteamCdn_DeveTerPrecedenciaMaxima()
        {
            // Arrange
            var stores = new List<RawgStoreItemDTO>
            {
                new()
                {
                    Url = "https://store.steampowered.com/app/1091500/Cyberpunk_2077/",
                    Store = new RawgNamedEntityDTO { Slug = "steam" }
                }
            };

            // Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd(
                rawgImageUrl: "https://media.rawg.io/media/crop/600/400/games/123.jpg",
                stores: stores,
                titulo: "Cyberpunk 2077",
                anoLancamento: 2020
            );

            // Assert
            result.Should().Be("https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1091500/library_600x900_2x.jpg");
        }

        [Theory]
        [InlineData("The Witcher 3: Wild Hunt", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/292030/library_600x900_2x.jpg")]
        [InlineData("Elden Ring", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/1245620/library_600x900_2x.jpg")]
        [InlineData("Chrono Trigger", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/613830/library_600x900_2x.jpg")]
        [InlineData("Slay the Spire 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2868840/library_600x900_2x.jpg")]
        [InlineData("Wreckfest 2", "https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/2011830/library_600x900_2x.jpg")]
        public void ResolverMelhorCapaHd_Camada2_ClassicosEObrasPrimasCuradas_DeveRetornarCapa600x900Hd(string titulo, string urlEsperada)
        {
            // Arrange & Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd(
                rawgImageUrl: "https://media.rawg.io/media/crop/600/400/games/retro.jpg",
                stores: null,
                titulo: titulo,
                anoLancamento: 1996
            );

            // Assert
            result.Should().Be(urlEsperada);
        }

        [Fact]
        public void ResolverMelhorCapaHd_Camada3_RawgFallback_DeveRemoverCropDeBaixaResolucao()
        {
            // Arrange
            var rawgCrop = "https://media.rawg.io/media/crop/600/400/games/indie/game.jpg";

            // Act
            var result = CatalogSanitizer.ResolverMelhorCapaHd(
                rawgImageUrl: rawgCrop,
                stores: null,
                titulo: "Indie Unknown 2024",
                anoLancamento: 2024
            );

            // Assert
            result.Should().Be("https://media.rawg.io/media/games/indie/game.jpg");
        }

        [Theory]
        [InlineData("Chrono Trigger", "snes", "https://thumbnails.libretro.com/Nintendo%20-%20Super%20Nintendo%20Entertainment%20System/Named_Boxarts/Chrono%20Trigger.png")]
        [InlineData("Castlevania: Symphony of the Night", "ps1", "https://thumbnails.libretro.com/Sony%20-%20PlayStation/Named_Boxarts/Castlevania_%20Symphony%20of%20the%20Night.png")]
        public void LibretroThumbnailService_DeveConstruirUrlBoxartValida(string titulo, string sistema, string urlEsperada)
        {
            // Act
            var url = LibretroThumbnailService.ObterUrlBoxart(titulo, sistema, 1997);

            // Assert
            url.Should().Be(urlEsperada);
        }
    }
}
