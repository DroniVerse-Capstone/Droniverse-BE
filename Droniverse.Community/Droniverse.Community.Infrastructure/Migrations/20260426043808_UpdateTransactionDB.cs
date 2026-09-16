using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<Guid>(
            //    name: "OrderID",
            //    table: "Transaction",
            //    type: "char(36)",
            //    nullable: true);

            //migrationBuilder.AddColumn<Guid>(
            //    name: "WithdrawRequestID",
            //    table: "Transaction",
            //    type: "char(36)",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Transaction_WithdrawRequestID",
            //    table: "Transaction",
            //    column: "WithdrawRequestID",
            //    unique: true);

            //migrationBuilder.AddCheckConstraint(
            //    name: "CK_Transaction_Type_Club_WithdrawRequest",
            //    table: "Transaction",
            //    sql: "((Type = 'COMMISSION' AND ClubID IS NOT NULL AND WithdrawRequestID IS NULL) OR (Type IN ('WITHDRAWAL', 'REFUND') AND WithdrawRequestID IS NOT NULL AND ClubID IS NULL))");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Transaction_WithdrawRequest_WithdrawRequestID",
            //    table: "Transaction",
            //    column: "WithdrawRequestID",
            //    principalTable: "WithdrawRequest",
            //    principalColumn: "WithdrawRequestID",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_WithdrawRequest_WithdrawRequestID",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_WithdrawRequestID",
                table: "Transaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transaction_Type_Club_WithdrawRequest",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "WithdrawRequestID",
                table: "Transaction");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transaction_Status",
                table: "Transaction",
                sql: "Status IN (0, 1, 2)");
        }
    }
}
