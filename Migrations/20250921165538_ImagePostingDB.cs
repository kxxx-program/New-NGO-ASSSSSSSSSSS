using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NGO_Web_Demo.Migrations
{
    /// <inheritdoc />
    public partial class ImagePostingDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Postings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ImageTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhotoURL = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Postings");
        }
    }
}
