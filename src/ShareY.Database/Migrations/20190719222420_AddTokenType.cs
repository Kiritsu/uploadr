using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareY.Database.Migrations
{
    public partial class AddTokenType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "token_type",
                table: "tokens",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "token_type",
                table: "tokens");
        }
    }
}
