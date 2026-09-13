using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLog_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrigramIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Jogos_Titulo\";");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "idx_jogos_titulo_trgm",
                table: "Jogos",
                column: "Titulo")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_jogos_titulo_trgm",
                table: "Jogos");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Titulo",
                table: "Jogos",
                column: "Titulo");
        }
    }
}
