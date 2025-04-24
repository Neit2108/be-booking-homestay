using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomestayBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Users_SenderId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Contacts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Users_SenderId",
                table: "Contacts",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Users_SenderId",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Users_SenderId",
                table: "Contacts",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
