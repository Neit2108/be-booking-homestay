using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomestayBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TestId",
                table: "TestCases",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestId",
                table: "TestCases");
        }
    }
}
