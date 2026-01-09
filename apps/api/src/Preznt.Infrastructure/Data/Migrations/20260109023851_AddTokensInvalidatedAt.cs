using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Preznt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTokensInvalidatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokensInvalidatedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokensInvalidatedAt",
                table: "users");
        }
    }
}
