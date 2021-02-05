using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class MissingPropsAndFacultativeEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shortenedurls_users_author_guid",
                table: "shortenedurls");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 709, DateTimeKind.Local).AddTicks(4934),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 734, DateTimeKind.Local).AddTicks(8877));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 722, DateTimeKind.Local).AddTicks(5334),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8787));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 722, DateTimeKind.Local).AddTicks(4588),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8380));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 724, DateTimeKind.Local).AddTicks(1329),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(4116));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 724, DateTimeKind.Local).AddTicks(855),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(3801));

            migrationBuilder.AddForeignKey(
                name: "fkey_user_authorid",
                table: "shortenedurls",
                column: "author_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fkey_user_authorid",
                table: "shortenedurls");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 734, DateTimeKind.Local).AddTicks(8877),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 709, DateTimeKind.Local).AddTicks(4934));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8787),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 722, DateTimeKind.Local).AddTicks(5334));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8380),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 722, DateTimeKind.Local).AddTicks(4588));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(4116),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 724, DateTimeKind.Local).AddTicks(1329));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(3801),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2021, 2, 5, 12, 26, 12, 724, DateTimeKind.Local).AddTicks(855));

            migrationBuilder.AddForeignKey(
                name: "FK_shortenedurls_users_author_guid",
                table: "shortenedurls",
                column: "author_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
