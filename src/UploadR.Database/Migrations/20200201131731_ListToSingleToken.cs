using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class ListToSingleToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tokens",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "users",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "token",
                table: "users");

            migrationBuilder.AddColumn<string[]>(
                name: "tokens",
                table: "users",
                type: "text[]",
                nullable: false,
                defaultValue: Array.Empty<object>());
        }
    }
}
