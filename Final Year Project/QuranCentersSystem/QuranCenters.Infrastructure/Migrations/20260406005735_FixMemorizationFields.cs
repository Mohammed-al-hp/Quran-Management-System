using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCenters.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMemorizationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ToAyah",
                table: "Memorizations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FromAyah",
                table: "Memorizations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CurrentSurah",
                table: "Memorizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DayName",
                table: "Memorizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Memorizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MemorizationQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemorizationId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemorizationQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemorizationQuestions_Memorizations_MemorizationId",
                        column: x => x.MemorizationId,
                        principalTable: "Memorizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemorizationQuestions_MemorizationId",
                table: "MemorizationQuestions",
                column: "MemorizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorizationQuestions");

            migrationBuilder.DropColumn(
                name: "CurrentSurah",
                table: "Memorizations");

            migrationBuilder.DropColumn(
                name: "DayName",
                table: "Memorizations");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Memorizations");

            migrationBuilder.AlterColumn<int>(
                name: "ToAyah",
                table: "Memorizations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FromAyah",
                table: "Memorizations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
