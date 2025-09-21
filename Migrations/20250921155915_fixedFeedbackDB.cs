using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class fixedFeedbackDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserEmail",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_UserEmail",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "Feedbacks");

            migrationBuilder.AddColumn<string>(
                name: "VolunteerID",
                table: "Feedbacks",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_VolunteerID",
                table: "Feedbacks",
                column: "VolunteerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks",
                column: "VolunteerID",
                principalTable: "Volunteers",
                principalColumn: "VolunteerID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Volunteers_VolunteerID",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_VolunteerID",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "VolunteerID",
                table: "Feedbacks");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "Feedbacks",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserEmail",
                table: "Feedbacks",
                column: "UserEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserEmail",
                table: "Feedbacks",
                column: "UserEmail",
                principalTable: "Users",
                principalColumn: "Email",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
