using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomestayBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTopRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TopRatePlaces",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TopRatePlaces");
        }
    }
}
