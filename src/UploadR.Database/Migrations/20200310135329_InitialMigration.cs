using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    email = table.Column<string>(nullable: false),
                    disabled = table.Column<bool>(nullable: false, defaultValue: false),
                    api_token_hash = table.Column<string>(nullable: false),
                    account_type = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_guid", x => x.guid);
                });

            migrationBuilder.CreateTable(
                name: "uploads",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    last_seen = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    download_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    removed = table.Column<bool>(nullable: false, defaultValue: false),
                    file_name = table.Column<string>(nullable: false),
                    content_type = table.Column<string>(nullable: false),
                    password_hash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fkey_user_authorid",
                        column: x => x.author_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_uploads_author_guid",
                table: "uploads",
                column: "author_guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uploads");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
