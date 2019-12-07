using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class InitialMigrations : Migration
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
                    disabled = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_guid", x => x.guid);
                    table.UniqueConstraint("ak_user_email", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    token_type = table.Column<int>(nullable: false, defaultValue: 0),
                    revoked = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_token_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fkey_token_userid",
                        column: x => x.user_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uploads",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    view_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    removed = table.Column<bool>(nullable: false, defaultValue: false),
                    file_name = table.Column<string>(nullable: false),
                    content_type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_upload_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fkey_upload_authorid",
                        column: x => x.author_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "index_user_id",
                table: "tokens",
                column: "user_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_uploads_author_guid",
                table: "uploads",
                column: "author_guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "uploads");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
