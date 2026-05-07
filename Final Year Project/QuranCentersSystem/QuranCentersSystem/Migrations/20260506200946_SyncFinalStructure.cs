using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCentersSystem.Migrations
{
    /// <inheritdoc />
    public partial class SyncFinalStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemorizationQuestions");

            migrationBuilder.DropTable(
                name: "Memorizations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "GroupAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CircleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAchievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SurahStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AyahStart = table.Column<int>(type: "int", nullable: false),
                    SurahEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AyahEnd = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeacherNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAchievements_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAchievements_StudentId",
                table: "StudentAchievements",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupAchievements");

            migrationBuilder.DropTable(
                name: "StudentAchievements");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Memorizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CurrentSurah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAyah = table.Column<int>(type: "int", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MistakesCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurahName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAyah = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memorizations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Memorizations_StudentId",
                table: "Memorizations",
                column: "StudentId");
        }
    }
}
