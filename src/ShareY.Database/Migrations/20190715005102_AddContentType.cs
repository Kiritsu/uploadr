using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareY.Database.Migrations
{
    public partial class AddContentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "uploads",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type",
                table: "uploads");
        }
    }
}
