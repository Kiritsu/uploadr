using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PsychicPotato.Database.Migrations
{
    public partial class last_seen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_seen",
                table: "uploads",
                nullable: false,
                defaultValueSql: "now()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_seen",
                table: "uploads");
        }
    }
}
