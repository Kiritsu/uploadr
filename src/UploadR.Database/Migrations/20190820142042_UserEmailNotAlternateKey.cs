using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class UserEmailNotAlternateKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "ak_user_email",
                table: "users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_user_email",
                table: "users",
                column: "email");
        }
    }
}
