using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //// 1. Gỡ bỏ Foreign Key đang trói buộc Index
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_WithdrawRequest_WithdrawRequestID",
                table: "Transaction");

            //// 2. Xóa Index Unique cũ
            migrationBuilder.DropIndex(
                name: "IX_Transaction_WithdrawRequestID",
                table: "Transaction");

            // 3. Tạo lại Index mới (Không Unique)
            //migrationBuilder.CreateIndex(
            //    name: "IX_Transaction_WithdrawRequestID",
            //    table: "Transaction",
            //    column: "WithdrawRequestID");

            // 4. Gắn lại Foreign Key
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
            // Làm ngược lại các bước trên khi muốn Rollback (Undo)
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_WithdrawRequest_WithdrawRequestID",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_WithdrawRequestID",
                table: "Transaction");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WithdrawRequestID",
                table: "Transaction",
                column: "WithdrawRequestID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_WithdrawRequest_WithdrawRequestID",
                table: "Transaction",
                column: "WithdrawRequestID",
                principalTable: "WithdrawRequest",
                principalColumn: "WithdrawRequestID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}