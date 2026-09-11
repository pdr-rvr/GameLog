using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class ListaServicesTests
    {
        [Fact]
        public async Task CriarLista_ComJogos_DevePersistirColecaoComItensOrdenados()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new ListaServices(context);

            var usuario = new Usuario { NomeUsuario = "ListCreator", Email = "list@gamelog.com", Senha = "hash", EstaAtivo = true };
            var jogo1 = new Jogo { Titulo = "Zelda BOTW", Descricao = "Adventure", Imagem = "botw.jpg", EstaAtivo = true };
            var jogo2 = new Jogo { Titulo = "Zelda TOTK", Descricao = "Adventure", Imagem = "totk.jpg", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            context.Jogos.AddRange(jogo1, jogo2);
            await context.SaveChangesAsync();

            var dto = new CriarListaDTO
            {
                Titulo = "Melhores Jogos do Switch",
                Descricao = "Minha seleção pessoal de exclusivos da Nintendo.",
                EstaPublica = true,
                JogosIds = new List<Guid> { jogo1.Id, jogo2.Id }
            };

            // Act
            var result = await service.CriarLista(usuario.Id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Titulo.Should().Be("Melhores Jogos do Switch");
            result.TotalJogos.Should().Be(2);
            result.Itens.Should().HaveCount(2);
            result.Itens[0].TituloJogo.Should().Be("Zelda BOTW");
            result.Itens[1].TituloJogo.Should().Be("Zelda TOTK");
        }

        [Fact]
        public async Task DeletarLista_DeveRemoverColecao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var service = new ListaServices(context);

            var usuario = new Usuario { NomeUsuario = "Deleter", Email = "del@gamelog.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var lista = await service.CriarLista(usuario.Id, new CriarListaDTO
            {
                Titulo = "Lista para Excluir",
                EstaPublica = true
            });

            // Act
            var removido = await service.DeletarLista(lista.ListaId, usuario.Id);

            // Assert
            removido.Should().BeTrue();
            var listaDb = await service.ObterListaPorId(lista.ListaId);
            listaDb.Should().BeNull();
        }
    }
}
