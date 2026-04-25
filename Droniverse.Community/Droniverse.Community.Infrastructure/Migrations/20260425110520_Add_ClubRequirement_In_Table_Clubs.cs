using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ClubRequirement_In_Table_Clubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClubID",
                table: "Transaction",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubRequirement",
                table: "ClubCreationRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubRequirement",
                table: "ClubAttemptRequest",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClubRequirement",
                table: "Club",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ClubID",
                table: "Transaction",
                column: "ClubID");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Club_ClubID",
                table: "Transaction",
                column: "ClubID",
                principalTable: "Club",
                principalColumn: "ClubID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Club_ClubID",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ClubID",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ClubID",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ClubRequirement",
                table: "ClubCreationRequest");

            migrationBuilder.DropColumn(
                name: "ClubRequirement",
                table: "ClubAttemptRequest");

            migrationBuilder.DropColumn(
                name: "ClubRequirement",
                table: "Club");
        }
    }
}
