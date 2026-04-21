using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_WithdrawRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WithdrawRequest",
                columns: table => new
                {
                    WithdrawRequestID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RequesterID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApproverID = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    WalletID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawRequest", x => x.WithdrawRequestID);
                    table.CheckConstraint("CK_WithdrawRequest_Status", "Status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELED')");
                    table.ForeignKey(
                        name: "FK_WithdrawRequest_Wallet_WalletID",
                        column: x => x.WalletID,
                        principalTable: "Wallet",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawRequest_WalletID",
                table: "WithdrawRequest",
                column: "WalletID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WithdrawRequest");
        }
    }
}
