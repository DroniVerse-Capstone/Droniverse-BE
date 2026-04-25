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
            // 1. Transaction.ClubID (có check tồn tại)
            migrationBuilder.Sql(@"
        SET @exist := (
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Transaction' 
            AND COLUMN_NAME = 'ClubID'
        );

        SET @sql := IF(@exist = 0, 
            'ALTER TABLE `Transaction` ADD COLUMN `ClubID` char(36) NULL;', 
            'SELECT 1;'
        );

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    ");

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
