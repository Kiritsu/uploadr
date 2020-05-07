using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class unverified_user : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "account_type",
                table: "users",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 734, DateTimeKind.Local).AddTicks(8877),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 813, DateTimeKind.Local).AddTicks(8989));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8787),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8686));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8380),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8128));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "shortenedurls",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(4116),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7888));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "shortenedurls",
                nullable: false,
                defaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(3801),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7416));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "account_type",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 813, DateTimeKind.Local).AddTicks(8989),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 734, DateTimeKind.Local).AddTicks(8877));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8686),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8787));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 827, DateTimeKind.Local).AddTicks(8128),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 746, DateTimeKind.Local).AddTicks(8380));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7888),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(4116));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "shortenedurls",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 19, 17, 27, 30, 830, DateTimeKind.Local).AddTicks(7416),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 5, 7, 7, 14, 46, 748, DateTimeKind.Local).AddTicks(3801));
        }
    }
}
