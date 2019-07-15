using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareY.Database.Migrations
{
    public partial class RemoveContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "uploads");

            migrationBuilder.DropColumn(
                name: "upload_type",
                table: "uploads");

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "uploads",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_name",
                table: "uploads");

            migrationBuilder.AddColumn<byte[]>(
                name: "content",
                table: "uploads",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddColumn<int>(
                name: "upload_type",
                table: "uploads",
                nullable: false,
                defaultValue: 0);
        }
    }
}
