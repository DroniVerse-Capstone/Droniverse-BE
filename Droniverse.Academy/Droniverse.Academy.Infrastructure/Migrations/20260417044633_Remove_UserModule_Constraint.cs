using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_UserModule_Constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tạo index riêng cho FK (QUAN TRỌNG NHẤT)
            migrationBuilder.CreateIndex(
                name: "IX_UserModule_ModuleID",
                table: "UserModule",
                column: "ModuleID");

            // 2. Drop index composite cũ (giờ FK đã có index riêng → OK)
            migrationBuilder.DropIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule");

            // 3. Tạo lại index NON-UNIQUE
            migrationBuilder.CreateIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule",
                columns: new[] { "ModuleID", "UserID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule");

            migrationBuilder.CreateIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule",
                columns: new[] { "ModuleID", "UserID" },
                unique: true);
        }
    }
}
