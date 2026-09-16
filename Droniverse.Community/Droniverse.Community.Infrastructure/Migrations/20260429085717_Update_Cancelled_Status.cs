using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Cancelled_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawRequest_Status",
                table: "WithdrawRequest",
                sql: "Status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WithdrawRequest_Status",
                table: "WithdrawRequest");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WithdrawRequest_Status",
                table: "WithdrawRequest",
                sql: "Status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELED')");
        }
    }
}
