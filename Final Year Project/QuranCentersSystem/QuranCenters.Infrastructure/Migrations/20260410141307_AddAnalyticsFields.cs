using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCenters.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MistakesCount",
                table: "Memorizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Memorizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DelayMinutes",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MistakesCount",
                table: "Memorizations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Memorizations");

            migrationBuilder.DropColumn(
                name: "DelayMinutes",
                table: "Attendances");
        }
    }
}
