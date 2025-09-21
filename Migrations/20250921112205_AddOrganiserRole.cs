using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganiserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "Member_PhotoURL",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganisationAddress",
                table: "Users",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganisationName",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganisationPhone",
                table: "Users",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Member_PhotoURL",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganisationAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganisationName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganisationPhone",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);
        }
    }
}
