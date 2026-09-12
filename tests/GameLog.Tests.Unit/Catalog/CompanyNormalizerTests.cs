using FluentAssertions;
using GameLog_Backend.Services.Catalog;
using Xunit;

namespace GameLog.Tests.Unit.Catalog
{
    public class CompanyNormalizerTests
    {
        [Theory]
        [InlineData("FromSoftware Inc.", "FromSoftware")]
        [InlineData("CD PROJEKT RED", "CD Projekt Red")]
        [InlineData("Square Enix Co., Ltd.", "Square Enix")]
        [InlineData("Electronic Arts Inc.", "Electronic Arts")]
        [InlineData("Sony Interactive Entertainment LLC", "PlayStation Studios")]
        [InlineData("Nintendo EPD", "Nintendo")]
        [InlineData("Capcom Co., Ltd.", "Capcom")]
        [InlineData("Rockstar Games Inc", "Rockstar Games")]
        public void NormalizarNomeEmpresa_DeveRetornarNomeCanonico(string nomeBruto, string esperado)
        {
            // Act
            var result = CompanyNormalizer.NormalizarNomeEmpresa(nomeBruto);

            // Assert
            result.Should().Be(esperado);
        }

        [Fact]
        public void ResolverParDesenvolvedoraPublicadora_FranquiaFromSoftware_DeveResolverCorretamente()
        {
            // Act
            var (dev, pub) = CompanyNormalizer.ResolverParDesenvolvedoraPublicadora("Elden Ring", null, "Bandai Namco");

            // Assert
            dev.Should().Be("FromSoftware");
            pub.Should().Be("Bandai Namco");
        }

        [Fact]
        public void ResolverParDesenvolvedoraPublicadora_TheWitcher_DeveResolverCdProjekt()
        {
            // Act
            var (dev, pub) = CompanyNormalizer.ResolverParDesenvolvedoraPublicadora("The Witcher 3: Wild Hunt", null, null);

            // Assert
            dev.Should().Be("CD Projekt Red");
            pub.Should().Be("CD Projekt Red");
        }
    }
}
