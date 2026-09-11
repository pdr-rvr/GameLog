using FluentAssertions;
using GameLog_Backend.Helpers;
using Xunit;

namespace GameLog.Tests.Unit.Helpers
{
    public class RelevanciaBuscaHelperTests
    {
        [Theory]
        [InlineData("God of War", "god of war", true)]
        [InlineData("God of War: Ragnarök", "god of war", true)]
        [InlineData("Dark Souls™: Remastered", "dark souls", true)]
        [InlineData("The Witcher 3: Wild Hunt", "witcher", true)]
        [InlineData("Super Mario Bros.", "mario", true)]
        [InlineData("FIFA 23", "call of duty", false)]
        public void CorrespondeBusca_DeveValidarCorretamente(string titulo, string query, bool esperado)
        {
            var result = RelevanciaBuscaHelper.CorrespondeBusca(titulo, null, null, query);
            result.Should().Be(esperado);
        }

        [Fact]
        public void RemoverAcentos_DeveNormalizarTexto()
        {
            var texto = "Pokémon: Let's Go, Pikachu! & Ragnarök - Ação e Emoção";
            var resultado = RelevanciaBuscaHelper.RemoverAcentos(texto);

            resultado.Should().Be("Pokemon: Let's Go, Pikachu! & Ragnarok - Acao e Emocao");
        }

        [Fact]
        public void CalcularScoreRelevancia_MatchExatoDeveTerScoreMaiorQueParcial()
        {
            var scoreExato = RelevanciaBuscaHelper.CalcularScoreRelevancia("Elden Ring", "elden ring");
            var scorePrefix = RelevanciaBuscaHelper.CalcularScoreRelevancia("Elden Ring: Shadow of the Erdtree", "elden ring");
            var scoreNaoRelacionado = RelevanciaBuscaHelper.CalcularScoreRelevancia("Minecraft", "elden ring");

            scoreExato.Should().BeGreaterThan(scorePrefix);
            scorePrefix.Should().BeGreaterThan(scoreNaoRelacionado);
            scoreNaoRelacionado.Should().Be(0);
        }
    }
}
