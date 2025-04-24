using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomestayBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlaceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Places",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Places");
        }
    }
}
