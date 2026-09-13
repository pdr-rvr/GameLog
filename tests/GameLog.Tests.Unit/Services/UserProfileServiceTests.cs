using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class UserProfileServiceTests
    {
        private UserProfileService CriarServico(GameLog_Backend.Database.GameLogContext context, AutoMapper.IMapper mapper)
        {
            var recomendacaoService = new RecomendacaoService(context, new MemoryCache(new MemoryCacheOptions()));
            return new UserProfileService(context, mapper, recomendacaoService);
        }

        [Fact]
        public async Task ListarUsuarios_DeveRetornarApenasUsuariosAtivos()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var u1 = new Usuario { NomeUsuario = "Ativo1", Email = "ativo1@test.com", Senha = "hash", EstaAtivo = true };
            var u2 = new Usuario { NomeUsuario = "Inativo", Email = "inativo@test.com", Senha = "hash", EstaAtivo = false };
            var u3 = new Usuario { NomeUsuario = "Ativo2", Email = "ativo2@test.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.AddRange(u1, u2, u3);
            await context.SaveChangesAsync();

            // Act
            var resultado = (await service.ListarUsuarios()).ToList();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(u => u.NomeUsuario == "Ativo1");
            resultado.Should().Contain(u => u.NomeUsuario == "Ativo2");
            resultado.Should().NotContain(u => u.NomeUsuario == "Inativo");
        }

        [Fact]
        public async Task ObterUsuarioPorId_ComIdValido_DeveRetornarUsuarioDTO()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var usuario = new Usuario { NomeUsuario = "GamerOne", Email = "gamer1@test.com", Senha = "hash", Bio = "Gamer Bio", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var resultado = await service.ObterUsuarioPorId(usuario.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NomeUsuario.Should().Be("GamerOne");
            resultado.Email.Should().Be("gamer1@test.com");
            resultado.Bio.Should().Be("Gamer Bio");
        }

        [Fact]
        public async Task ObterUsuarioPorId_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            // Act
            var resultado = await service.ObterUsuarioPorId(Guid.NewGuid());

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task EditarUsuario_ComSenhaIncorreta_DeveRetornarNull()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCorreta123!", 11);
            var usuario = new Usuario { NomeUsuario = "UserEdit", Email = "edit@test.com", Senha = senhaHash, EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new EditarUsuarioDTO
            {
                SenhaAtual = "SenhaErrada999!",
                Bio = "Nova Bio"
            };

            // Act
            var resultado = await service.EditarUsuario(usuario.Id, dto.SenhaAtual, dto);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task EditarUsuario_ComSenhaCorreta_DeveAtualizarBioENome()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCorreta123!", 11);
            var usuario = new Usuario { NomeUsuario = "OldName", Email = "old@test.com", Senha = senhaHash, EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var dto = new EditarUsuarioDTO
            {
                SenhaAtual = "SenhaCorreta123!",
                NomeUsuario = "NewName",
                Bio = "Nova Bio Atualizada"
            };

            // Act
            var resultado = await service.EditarUsuario(usuario.Id, dto.SenhaAtual, dto);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.NomeUsuario.Should().Be("NewName");
            resultado.Bio.Should().Be("Nova Bio Atualizada");

            var usuarioDb = await context.Usuarios.FindAsync(usuario.Id);
            usuarioDb!.NomeUsuario.Should().Be("NewName");
            usuarioDb.Bio.Should().Be("Nova Bio Atualizada");
        }

        [Fact]
        public async Task DeletarUsuario_ComSenhaCorreta_DeveDesativarUsuarioERetornarTrue()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaSegura123!", 11);
            var usuario = new Usuario { NomeUsuario = "ToDelete", Email = "delete@test.com", Senha = senhaHash, EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var sucesso = await service.DeletarUsuario(usuario.Id, "SenhaSegura123!");

            // Assert
            sucesso.Should().BeTrue();
            var usuarioDb = await context.Usuarios.FindAsync(usuario.Id);
            usuarioDb!.EstaAtivo.Should().BeFalse();
        }

        [Fact]
        public async Task DeletarUsuario_ComSenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaSegura123!", 11);
            var usuario = new Usuario { NomeUsuario = "NotDelete", Email = "notdelete@test.com", Senha = senhaHash, EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var sucesso = await service.DeletarUsuario(usuario.Id, "SenhaInvalida!");

            // Assert
            sucesso.Should().BeFalse();
            var usuarioDb = await context.Usuarios.FindAsync(usuario.Id);
            usuarioDb!.EstaAtivo.Should().BeTrue();
        }

        [Fact]
        public async Task EmailEmUso_DeveRetornarTrueQuandoExisteEAtivo()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var service = CriarServico(context, mapper);

            var usuario = new Usuario { NomeUsuario = "EmailCheck", Email = "check@test.com", Senha = "hash", EstaAtivo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var existe = await service.EmailEmUso("CHECK@TEST.COM");
            var naoExiste = await service.EmailEmUso("outro@test.com");

            // Assert
            existe.Should().BeTrue();
            naoExiste.Should().BeFalse();
        }
    }
}
