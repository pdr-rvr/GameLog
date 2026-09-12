using FluentAssertions;
using GameLog_Backend.Services.Catalog;
using Xunit;

namespace GameLog.Tests.Unit.Catalog
{
    public class GenreTaxonomyServiceTests
    {
        [Fact]
        public void CategoriasCanonicas_DeveConterCategoriasEssenciais()
        {
            // Assert
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Ação");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("RPG");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Soulslike");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Metroidvania");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Tiro (FPS / TPS)");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Terror & Sobrevivência");
            GenreTaxonomyService.CategoriasCanonicas.Should().Contain("Ficção Científica & Cyberpunk");
        }

        [Theory]
        [InlineData("action", "Ação")]
        [InlineData("role-playing-games-rpg", "RPG")]
        [InlineData("shooter", "Tiro (FPS / TPS)")]
        [InlineData("strategy", "Estratégia")]
        [InlineData("massively-multiplayer", "MMORPG")]
        public void MapearGenerosETags_GênerosRAWGBasicos_DeveMapearCorretamente(string rawgGenre, string expected)
        {
            // Act
            var result = GenreTaxonomyService.MapearGenerosETags(new[] { rawgGenre }, null);

            // Assert
            result.Should().Contain(expected);
        }

        [Theory]
        [InlineData("souls-like", "Soulslike")]
        [InlineData("metroidvania", "Metroidvania")]
        [InlineData("jrpg", "JRPG")]
        [InlineData("fps", "Tiro (FPS / TPS)")]
        [InlineData("survival-horror", "Survival Horror")]
        [InlineData("sci-fi", "Ficção Científica & Cyberpunk")]
        [InlineData("cyberpunk", "Ficção Científica & Cyberpunk")]
        public void MapearGenerosETags_TagsEspecializadas_DeveInferirSubgeneros(string rawgTag, string expected)
        {
            // Act
            var result = GenreTaxonomyService.MapearGenerosETags(null, new[] { rawgTag });

            // Assert
            result.Should().Contain(expected);
        }

        [Fact]
        public void MapearGenerosETags_InferenciaContextualPorTitulo_DeveAdicionarCategoriasCorretas()
        {
            // Act
            var darkSouls = GenreTaxonomyService.MapearGenerosETags(new[] { "action" }, null, "Dark Souls III");
            var hollowKnight = GenreTaxonomyService.MapearGenerosETags(new[] { "adventure" }, null, "Hollow Knight");
            var cyberpunk = GenreTaxonomyService.MapearGenerosETags(new[] { "rpg" }, null, "Cyberpunk 2077");

            // Assert
            darkSouls.Should().Contain("Soulslike");
            darkSouls.Should().Contain("RPG de Ação");

            hollowKnight.Should().Contain("Metroidvania");

            cyberpunk.Should().Contain("Ficção Científica & Cyberpunk");
        }

        [Fact]
        public void MapearGenerosETags_SemGenerosOuTags_DeveRetornarAcaoComoFallback()
        {
            // Act
            var result = GenreTaxonomyService.MapearGenerosETags(null, null, null);

            // Assert
            result.Should().ContainSingle(g => g == "Ação");
        }
    }
}
