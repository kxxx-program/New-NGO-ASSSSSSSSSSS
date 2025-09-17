using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class CreateEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventID = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    EventTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventStartDate = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: false),
                    EventEndDate = table.Column<DateTime>(type: "datetime2", maxLength: 200, nullable: false),
                    EventStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EventEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EventStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventPhotoURL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
