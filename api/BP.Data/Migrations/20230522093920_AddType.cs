using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Sensor");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Sensor",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Sensor");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Sensor",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
