using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShareY.Database.Migrations
{
    public partial class InitialMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    email = table.Column<string>(nullable: false),
                    disabled = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_id", x => x.id);
                    table.UniqueConstraint("ak_user_email", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("key_token_guid", x => x.guid);
                    table.ForeignKey(
                        name: "fkey_token_userid",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uploads",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    author_id = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    download_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    visible = table.Column<bool>(nullable: false, defaultValue: true),
                    removed = table.Column<bool>(nullable: false, defaultValue: false),
                    upload_type = table.Column<int>(nullable: false),
                    content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("key_upload_id", x => x.id);
                    table.ForeignKey(
                        name: "fkey_upload_authorid",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "index_user_id",
                table: "tokens",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_uploads_author_id",
                table: "uploads",
                column: "author_id");
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
