using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCenters.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingParentEmail : Migration
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

        }
    }
}
