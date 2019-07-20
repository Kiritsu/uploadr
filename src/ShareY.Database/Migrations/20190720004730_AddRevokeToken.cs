using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareY.Database.Migrations
{
    public partial class AddRevokeToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "revoked",
                table: "tokens",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "revoked",
                table: "tokens");
        }
    }
}
