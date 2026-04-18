using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCentersSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCircleWithSchedulingAndFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CircleType",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CircleType",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Circles");
        }
    }
}
