using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCenters.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMemorizationStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Parent",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Parent");
        }
    }
}
