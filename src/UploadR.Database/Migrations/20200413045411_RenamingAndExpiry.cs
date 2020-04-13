using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class RenamingAndExpiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "download_count",
                table: "uploads");

            migrationBuilder.RenameColumn(
                name: "api_token_hash",
                table: "users",
                newName: "api_token");

            migrationBuilder.AlterColumn<int>(
                name: "account_type",
                table: "users",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 489, DateTimeKind.Local).AddTicks(3467),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(4074),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                nullable: false,
                defaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(3443),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "expiry_time",
                table: "uploads",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<long>(
                name: "seen_count",
                table: "uploads",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry_time",
                table: "uploads");

            migrationBuilder.DropColumn(
                name: "seen_count",
                table: "uploads");

            migrationBuilder.RenameColumn(
                name: "api_token",
                table: "users",
                newName: "api_token_hash");

            migrationBuilder.AlterColumn<int>(
                name: "account_type",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 489, DateTimeKind.Local).AddTicks(3467));

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(4074));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "uploads",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2020, 4, 13, 6, 54, 11, 504, DateTimeKind.Local).AddTicks(3443));

            migrationBuilder.AddColumn<long>(
                name: "download_count",
                table: "uploads",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
