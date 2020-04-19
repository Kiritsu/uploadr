using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class shortenentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 813, DateTimeKind.Local).AddTicks(8989),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 489, DateTimeKind.Local).AddTicks(3467));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8686),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(4074));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8128),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(3443));

            migrationBuilder.CreateTable(
                name: "shortenedurls",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7416)),
                    last_seen = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7888)),
                    expiry_time = table.Column<TimeSpan>(nullable: false, defaultValue: new TimeSpan(0, 0, 0, 0, 0)),
                    removed = table.Column<bool>(nullable: false, defaultValue: false),
                    password_hash = table.Column<string>(nullable: true),
                    seen_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    url = table.Column<string>(nullable: false),
                    shorten = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shortenedurl_guid", x => x.guid);
                    table.ForeignKey(
                        name: "FK_shortenedurls_users_author_guid",
                        column: x => x.author_guid,
                        principalTable: "users",
                        principalColumn: "guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shortenedurls_author_guid",
                table: "shortenedurls",
                column: "author_guid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shortenedurls");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 489, DateTimeKind.Local).AddTicks(3467),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 813, DateTimeKind.Local).AddTicks(8989));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(4074),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8686));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(3443),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8128));
        }
    }
}
