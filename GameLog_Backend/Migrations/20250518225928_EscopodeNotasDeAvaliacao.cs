using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLog_Backend.Migrations
{
    /// <inheritdoc />
    public partial class EscopodeNotasDeAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Avaliacao_Nota_Range",
                table: "Avaliacoes",
                sql: "[Nota] >= 0 AND [Nota] <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Avaliacao_Nota_Range",
                table: "Avaliacoes");
        }
    }
}
