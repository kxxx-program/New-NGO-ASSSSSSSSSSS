using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class FullFeedbackDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "VolunteerID",
                table: "Feedbacks",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks",
                column: "VolunteerID",
                principalTable: "Volunteers",
                principalColumn: "VolunteerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<string>(
                name: "VolunteerID",
                table: "Feedbacks",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks",
                column: "VolunteerID",
                principalTable: "Volunteers",
                principalColumn: "VolunteerID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
