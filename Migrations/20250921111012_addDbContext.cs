using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class addDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Events",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    VolunteerID = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.VolunteerID);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VolunteerID = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    EventID = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkHours = table.Column<int>(type: "int", nullable: false),
                    EventCompletion = table.Column<int>(type: "int", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerEvents_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerEvents_Volunteers_VolunteerID",
                        column: x => x.VolunteerID,
                        principalTable: "Volunteers",
                        principalColumn: "VolunteerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerEvents_EventID",
                table: "VolunteerEvents",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerEvents_VolunteerID",
                table: "VolunteerEvents",
                column: "VolunteerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerEvents");

            migrationBuilder.DropTable(
                name: "Volunteers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Events");
        }
    }
}
