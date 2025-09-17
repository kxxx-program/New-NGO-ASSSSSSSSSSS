using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class EventsDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "EventStartDate",
                table: "Events",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EventEndDate",
                table: "Events",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EventStartDate",
                table: "Events",
                type: "datetime2",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventEndDate",
                table: "Events",
                type: "datetime2",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
