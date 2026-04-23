using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCentersSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddParentEmailToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "Students");
        }
    }
}
