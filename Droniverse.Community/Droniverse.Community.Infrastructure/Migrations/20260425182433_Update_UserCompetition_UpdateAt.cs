using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserCompetition_UpdateAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRound",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserRound");
        }
    }
}
