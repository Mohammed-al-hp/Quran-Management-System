using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuranCentersSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCircleSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndPrayer",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Circles",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedDays",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartPrayer",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Circles",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimingType",
                table: "Circles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndPrayer",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "SelectedDays",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "StartPrayer",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Circles");

            migrationBuilder.DropColumn(
                name: "TimingType",
                table: "Circles");
        }
    }
}
