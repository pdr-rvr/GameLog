using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLog_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeEmpresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.EmpresaId);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    GeneroId = table.Column<Guid>(type: "uuid", nullable: false),
                    TituloGenero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.GeneroId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeUsuario = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FotoDePerfil = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Imagem = table.Column<string>(type: "text", nullable: true),
                    DataLancamento = table.Column<DateOnly>(type: "date", nullable: false),
                    ClassificacaoIndicativa = table.Column<int>(type: "integer", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicadoraId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.JogoId);
                    table.ForeignKey(
                        name: "FK_Jogos_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "EmpresaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Empresa_PublicadoraId",
                        column: x => x.PublicadoraId,
                        principalTable: "Empresa",
                        principalColumn: "EmpresaId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ListasDeJogos",
                columns: table => new
                {
                    ListaDeJogosId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstaPublica = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasDeJogos", x => x.ListaDeJogosId);
                    table.ForeignKey(
                        name: "FK_ListasDeJogos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevogadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoPorIp = table.Column<string>(type: "text", nullable: true),
                    RevogadoPorIp = table.Column<string>(type: "text", nullable: true),
                    SubstituidoPorToken = table.Column<string>(type: "text", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegueUsuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioSeguidorId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioSeguidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegueUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegueUsuarios_Usuarios_UsuarioSeguidoId",
                        column: x => x.UsuarioSeguidoId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegueUsuarios_Usuarios_UsuarioSeguidorId",
                        column: x => x.UsuarioSeguidorId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    AvaliacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TextoAvaliacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataPublicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.AvaliacaoId);
                    table.CheckConstraint("CK_Avaliacao_Nota_Range", "\"Nota\" >= 0 AND \"Nota\" <= 5");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "JogoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensBiblioteca",
                columns: table => new
                {
                    BibliotecaJogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensBiblioteca", x => x.BibliotecaJogoId);
                    table.ForeignKey(
                        name: "FK_ItensBiblioteca_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "JogoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensBiblioteca_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JogoGenero",
                columns: table => new
                {
                    GenerosId = table.Column<Guid>(type: "uuid", nullable: false),
                    JogosId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogoGenero", x => new { x.GenerosId, x.JogosId });
                    table.ForeignKey(
                        name: "FK_JogoGenero_Generos_GenerosId",
                        column: x => x.GenerosId,
                        principalTable: "Generos",
                        principalColumn: "GeneroId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JogoGenero_Jogos_JogosId",
                        column: x => x.JogosId,
                        principalTable: "Jogos",
                        principalColumn: "JogoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JogosFavoritosUsuarios",
                columns: table => new
                {
                    JogoFavoritoUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Posicao = table.Column<int>(type: "integer", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogosFavoritosUsuarios", x => x.JogoFavoritoUsuarioId);
                    table.ForeignKey(
                        name: "FK_JogosFavoritosUsuarios_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "JogoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JogosFavoritosUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensDeListas",
                columns: table => new
                {
                    ItemDeListaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListaDeJogosId = table.Column<Guid>(type: "uuid", nullable: false),
                    JogoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataAdicionado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensDeListas", x => x.ItemDeListaId);
                    table.ForeignKey(
                        name: "FK_ItensDeListas_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "JogoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensDeListas_ListasDeJogos_ListaDeJogosId",
                        column: x => x.ListaDeJogosId,
                        principalTable: "ListasDeJogos",
                        principalColumn: "ListaDeJogosId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurtidasDeAvaliacoes",
                columns: table => new
                {
                    CurtidaDeAvaliacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Curtida = table.Column<bool>(type: "boolean", nullable: false),
                    AvaliacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurtidasDeAvaliacoes", x => x.CurtidaDeAvaliacaoId);
                    table.ForeignKey(
                        name: "FK_CurtidasDeAvaliacoes_Avaliacoes_AvaliacaoId",
                        column: x => x.AvaliacaoId,
                        principalTable: "Avaliacoes",
                        principalColumn: "AvaliacaoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurtidasDeAvaliacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespostasDeAvaliacao",
                columns: table => new
                {
                    RespostaDeAvaliacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AvaliacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasDeAvaliacao", x => x.RespostaDeAvaliacaoId);
                    table.ForeignKey(
                        name: "FK_RespostasDeAvaliacao_Avaliacoes_AvaliacaoId",
                        column: x => x.AvaliacaoId,
                        principalTable: "Avaliacoes",
                        principalColumn: "AvaliacaoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespostasDeAvaliacao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurtidasDeRespostas",
                columns: table => new
                {
                    CurtidaDeRespostaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Curtida = table.Column<bool>(type: "boolean", nullable: false),
                    RespostaDeAvaliacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurtidasDeRespostas", x => x.CurtidaDeRespostaId);
                    table.ForeignKey(
                        name: "FK_CurtidasDeRespostas_RespostasDeAvaliacao_RespostaDeAvaliaca~",
                        column: x => x.RespostaDeAvaliacaoId,
                        principalTable: "RespostasDeAvaliacao",
                        principalColumn: "RespostaDeAvaliacaoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurtidasDeRespostas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_DataPublicacao",
                table: "Avaliacoes",
                column: "DataPublicacao");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_EstaAtivo",
                table: "Avaliacoes",
                column: "EstaAtivo");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_JogoId",
                table: "Avaliacoes",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_UsuarioId_JogoId",
                table: "Avaliacoes",
                columns: new[] { "UsuarioId", "JogoId" },
                unique: true,
                filter: "\"EstaAtivo\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasDeAvaliacoes_AvaliacaoId",
                table: "CurtidasDeAvaliacoes",
                column: "AvaliacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasDeAvaliacoes_UsuarioId",
                table: "CurtidasDeAvaliacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasDeRespostas_RespostaDeAvaliacaoId",
                table: "CurtidasDeRespostas",
                column: "RespostaDeAvaliacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasDeRespostas_UsuarioId",
                table: "CurtidasDeRespostas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_NomeEmpresa",
                table: "Empresa",
                column: "NomeEmpresa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Generos_TituloGenero",
                table: "Generos",
                column: "TituloGenero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensBiblioteca_DataAtualizacao",
                table: "ItensBiblioteca",
                column: "DataAtualizacao");

            migrationBuilder.CreateIndex(
                name: "IX_ItensBiblioteca_EstaAtivo",
                table: "ItensBiblioteca",
                column: "EstaAtivo");

            migrationBuilder.CreateIndex(
                name: "IX_ItensBiblioteca_JogoId",
                table: "ItensBiblioteca",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensBiblioteca_UsuarioId_JogoId",
                table: "ItensBiblioteca",
                columns: new[] { "UsuarioId", "JogoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensDeListas_JogoId",
                table: "ItensDeListas",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensDeListas_ListaDeJogosId_JogoId",
                table: "ItensDeListas",
                columns: new[] { "ListaDeJogosId", "JogoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JogoGenero_JogosId",
                table: "JogoGenero",
                column: "JogosId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_DataLancamento",
                table: "Jogos",
                column: "DataLancamento");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_EmpresaId",
                table: "Jogos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_EstaAtivo",
                table: "Jogos",
                column: "EstaAtivo");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_EstaAtivo_DataLancamento",
                table: "Jogos",
                columns: new[] { "EstaAtivo", "DataLancamento" });

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_PublicadoraId",
                table: "Jogos",
                column: "PublicadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Titulo",
                table: "Jogos",
                column: "Titulo");

            migrationBuilder.CreateIndex(
                name: "IX_JogosFavoritosUsuarios_JogoId",
                table: "JogosFavoritosUsuarios",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosFavoritosUsuarios_UsuarioId_Posicao",
                table: "JogosFavoritosUsuarios",
                columns: new[] { "UsuarioId", "Posicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListasDeJogos_UsuarioId",
                table: "ListasDeJogos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasDeAvaliacao_AvaliacaoId",
                table: "RespostasDeAvaliacao",
                column: "AvaliacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasDeAvaliacao_UsuarioId",
                table: "RespostasDeAvaliacao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SegueUsuarios_UsuarioSeguidoId",
                table: "SegueUsuarios",
                column: "UsuarioSeguidoId");

            migrationBuilder.CreateIndex(
                name: "IX_SegueUsuarios_UsuarioSeguidorId",
                table: "SegueUsuarios",
                column: "UsuarioSeguidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NomeUsuario",
                table: "Usuarios",
                column: "NomeUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurtidasDeAvaliacoes");

            migrationBuilder.DropTable(
                name: "CurtidasDeRespostas");

            migrationBuilder.DropTable(
                name: "ItensBiblioteca");

            migrationBuilder.DropTable(
                name: "ItensDeListas");

            migrationBuilder.DropTable(
                name: "JogoGenero");

            migrationBuilder.DropTable(
                name: "JogosFavoritosUsuarios");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SegueUsuarios");

            migrationBuilder.DropTable(
                name: "RespostasDeAvaliacao");

            migrationBuilder.DropTable(
                name: "ListasDeJogos");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Empresa");
        }
    }
}
