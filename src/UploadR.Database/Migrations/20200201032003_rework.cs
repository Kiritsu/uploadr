using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadR.Database.Migrations
{
    public partial class rework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fkey_upload_authorid",
                table: "uploads");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.RenameColumn(
                name: "view_count",
                table: "uploads",
                newName: "download_count");

            migrationBuilder.AddColumn<List<string>>(
                name: "tokens",
                table: "users",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "account_type",
                table: "users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fkey_user_authorid",
                table: "uploads",
                column: "author_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fkey_user_authorid",
                table: "uploads");

            migrationBuilder.DropColumn(
                name: "tokens",
                table: "users");

            migrationBuilder.DropColumn(
                name: "account_type",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "download_count",
                table: "uploads",
                newName: "view_count");

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    revoked = table.Column<bool>(nullable: false, defaultValue: false),
                    token_type = table.Column<int>(nullable: false, defaultValue: 0),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "index_user_id",
                table: "tokens",
                column: "user_guid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fkey_upload_authorid",
                table: "uploads",
                column: "author_guid",
                principalTable: "users",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
