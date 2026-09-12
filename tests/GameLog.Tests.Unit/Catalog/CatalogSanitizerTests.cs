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
        [InlineData("Dark Souls: Remastered", "darksouls")]
        [InlineData("Persona 5: Royal (Definitive Edition)", "persona5royal")]
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
    }
}
