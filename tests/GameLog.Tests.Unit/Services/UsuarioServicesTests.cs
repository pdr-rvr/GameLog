using FluentAssertions;
using GameLog.Tests.Unit.Helpers;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services;
using Xunit;

namespace GameLog.Tests.Unit.Services
{
    public class UsuarioServicesTests
    {
        [Fact]
        public async Task CriarUsuario_ComDadosValidos_DeveSalvarUsuarioEHashDaSenha()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            var dto = new CriarUsuarioDTO
            {
                NomeUsuario = "GamerMaster",
                Email = "gamer@gamelog.com",
                Senha = "Password123!",
                Bio = "Gamer apaixonado por RPGs"
            };

            // Act
            var result = await service.CriarUsuario(dto);

            // Assert
            result.Should().NotBeNull();
            result.NomeUsuario.Should().Be("GamerMaster");
            result.Email.Should().Be("gamer@gamelog.com");
            result.EstaAtivo.Should().BeTrue();

            var usuarioDb = await context.Usuarios.FindAsync(result.UsuarioId);
            usuarioDb.Should().NotBeNull();
            usuarioDb!.Senha.Should().NotBe("Password123!");
            usuarioDb.Senha.Should().StartWith("$2");
        }

        [Fact]
        public async Task CriarUsuario_ComEmailDuplicado_DeveLancarExcecao()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            var dto1 = new CriarUsuarioDTO
            {
                NomeUsuario = "UserOne",
                Email = "duplicate@gamelog.com",
                Senha = "Password123!"
            };
            await service.CriarUsuario(dto1);

            var dto2 = new CriarUsuarioDTO
            {
                NomeUsuario = "UserTwo",
                Email = "duplicate@gamelog.com",
                Senha = "Password123!"
            };

            // Act & Assert
            var act = async () => await service.CriarUsuario(dto2);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*e-mail já está em uso*");
        }

        [Fact]
        public async Task AutenticarUsuario_ComCredenciaisValidas_DeveRetornarTokenJWT()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "LoginUser",
                Email = "login@gamelog.com",
                Senha = "SecretPassword123!"
            });

            var loginDTO = new UsuarioLoginDTO
            {
                Email = "login@gamelog.com",
                Senha = "SecretPassword123!"
            };

            // Act
            var (usuario, token, expiraEm) = await service.AutenticarUsuario(loginDTO);

            // Assert
            usuario.Should().NotBeNull();
            usuario!.NomeUsuario.Should().Be("LoginUser");
            token.Should().NotBeNullOrWhiteSpace();
            expiraEm.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task AutenticarUsuario_ComSenhaIncorreta_DeveRetornarNull()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "LoginUser2",
                Email = "login2@gamelog.com",
                Senha = "SecretPassword123!"
            });

            var loginDTO = new UsuarioLoginDTO
            {
                Email = "login2@gamelog.com",
                Senha = "WrongPassword999!"
            };

            // Act
            var (usuario, token, _) = await service.AutenticarUsuario(loginDTO);

            // Assert
            usuario.Should().BeNull();
            token.Should().BeNull();
        }

        [Fact]
        public async Task SeguirUsuario_DeveCriarRelacionamento_ENaoPermitirAutoSeguir()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            var user1 = await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "UserA",
                Email = "usera@gamelog.com",
                Senha = "Password123!"
            });

            var user2 = await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "UserB",
                Email = "userb@gamelog.com",
                Senha = "Password123!"
            });

            // Act: user1 segue user2
            var (seguiu, total) = await service.AlternarSeguirUsuario(user1.UsuarioId, user2.UsuarioId);
            seguiu.Should().BeTrue();
            total.Should().Be(1);

            var seguidores = await service.ObterSeguidores(user2.UsuarioId);
            seguidores.Should().ContainSingle(s => s.UsuarioId == user1.UsuarioId);

            // Act & Assert: Auto-seguir deve lançar InvalidOperationException
            var autoSeguir = async () => await service.AlternarSeguirUsuario(user1.UsuarioId, user1.UsuarioId);
            await autoSeguir.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*não pode seguir seu próprio perfil*");
        }

        [Fact]
        public async Task DeixarDeSeguir_DeveRemoverRelacionamento()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            var user1 = await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "Follower",
                Email = "follower@gamelog.com",
                Senha = "Password123!"
            });

            var user2 = await service.CriarUsuario(new CriarUsuarioDTO
            {
                NomeUsuario = "Leader",
                Email = "leader@gamelog.com",
                Senha = "Password123!"
            });

            await service.AlternarSeguirUsuario(user1.UsuarioId, user2.UsuarioId);

            // Act: toggle follow de novo para des-seguir
            var (seguiu, total) = await service.AlternarSeguirUsuario(user1.UsuarioId, user2.UsuarioId);

            // Assert
            seguiu.Should().BeFalse();
            total.Should().Be(0);
            var seguidores = await service.ObterSeguidores(user2.UsuarioId);
            seguidores.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterTimelineAtividades_DeveRetornarEventosDeSeguidos()
        {
            // Arrange
            using var context = TestContextHelper.CreateInMemoryContext();
            var mapper = TestContextHelper.CreateMapper();
            var jwtOptions = TestContextHelper.CreateJwtSettings();
            var service = new UsuarioServices(context, mapper, jwtOptions);

            var user1 = await service.CriarUsuario(new CriarUsuarioDTO { NomeUsuario = "UserA", Email = "a@test.com", Senha = "Password123!" });
            var user2 = await service.CriarUsuario(new CriarUsuarioDTO { NomeUsuario = "UserB", Email = "b@test.com", Senha = "Password123!" });

            // UserA segue UserB
            await service.AlternarSeguirUsuario(user1.UsuarioId, user2.UsuarioId);

            var empresa = new Empresa { NomeEmpresa = "TestStudio", EstaAtivo = true };
            context.Empresa.Add(empresa);

            var jogo = new Jogo { Titulo = "Timeline Game", Imagem = "img.jpg", DataLancamento = new DateOnly(2024, 1, 1), Empresa = empresa, EstaAtivo = true };
            context.Jogos.Add(jogo);

            var user2Entity = await context.Usuarios.FindAsync(user2.UsuarioId);
            var avaliacao = new Avaliacao { Jogo = jogo, Usuario = user2Entity!, Nota = 5, TextoAvaliacao = "Incrivel!", DataPublicacao = DateTime.UtcNow, EstaAtivo = true };
            context.Avaliacoes.Add(avaliacao);
            await context.SaveChangesAsync();

            // Act
            var timeline = await service.ObterTimelineAtividades(user1.UsuarioId);

            // Assert
            timeline.Should().ContainSingle();
            var act = timeline.First();
            act.Tipo.Should().Be("Avaliou");
            act.UsuarioNome.Should().Be("UserB");
            act.JogoTitulo.Should().Be("Timeline Game");
        }
    }
}
