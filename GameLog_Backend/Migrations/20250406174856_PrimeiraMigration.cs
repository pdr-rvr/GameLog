using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLog_Backend.Migrations
{
    /// <inheritdoc />
    public partial class PrimeiraMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeEmpresa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.EmpresaId);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    GeneroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TituloGenero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.GeneroId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeUsuario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FotoDePerfil = table.Column<string>(type: "varchar(MAX)", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    JogoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Imagem = table.Column<string>(type: "varchar(MAX)", nullable: true),
                    DataLancamento = table.Column<DateOnly>(type: "date", nullable: false),
                    ClassificacaoIndicativa = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.JogoId);
                    table.ForeignKey(
                        name: "FK_Jogos_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresa",
                        principalColumn: "EmpresaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegueUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioSeguidorId = table.Column<int>(type: "int", nullable: false),
                    UsuarioSeguidoId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
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
                    AvaliacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    JogoId = table.Column<int>(type: "int", nullable: false),
                    TextoAvaliacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.AvaliacaoId);
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
                name: "JogoGenero",
                columns: table => new
                {
                    GenerosId = table.Column<int>(type: "int", nullable: false),
                    JogosId = table.Column<int>(type: "int", nullable: false)
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
                name: "CurtidasDeAvaliacoes",
                columns: table => new
                {
                    CurtidaDeAvaliacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Curtida = table.Column<bool>(type: "bit", nullable: false),
                    AvaliacaoId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "RespostasDeAvaliacao",
                columns: table => new
                {
                    RespostaDeAvaliacaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comentario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AvaliacaoId = table.Column<int>(type: "int", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "bit", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_JogoId",
                table: "Avaliacoes",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_UsuarioId",
                table: "Avaliacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasDeAvaliacoes_AvaliacaoId",
                table: "CurtidasDeAvaliacoes",
                column: "AvaliacaoId");

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
                name: "IX_JogoGenero_JogosId",
                table: "JogoGenero",
                column: "JogosId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_EmpresaId",
                table: "Jogos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasDeAvaliacao_AvaliacaoId",
                table: "RespostasDeAvaliacao",
                column: "AvaliacaoId");

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
                name: "JogoGenero");

            migrationBuilder.DropTable(
                name: "RespostasDeAvaliacao");

            migrationBuilder.DropTable(
                name: "SegueUsuarios");

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
