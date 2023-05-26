using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BP.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Reading_DateTime_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reading_DateTime",
                table: "Reading",
                column: "DateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reading_DateTime",
                table: "Reading");
        }
    }
}
