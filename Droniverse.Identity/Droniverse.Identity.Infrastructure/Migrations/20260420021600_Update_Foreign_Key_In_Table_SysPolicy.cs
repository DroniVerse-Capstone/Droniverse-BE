using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Foreign_Key_In_Table_SysPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SysPolicy_CreatedBy",
                table: "SysPolicy",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SysPolicy_UpdatedBy",
                table: "SysPolicy",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_SysPolicy_Account_CreatedBy",
                table: "SysPolicy",
                column: "CreatedBy",
                principalTable: "Account",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SysPolicy_Account_UpdatedBy",
                table: "SysPolicy",
                column: "UpdatedBy",
                principalTable: "Account",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SysPolicy_Account_CreatedBy",
                table: "SysPolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_SysPolicy_Account_UpdatedBy",
                table: "SysPolicy");

            migrationBuilder.DropIndex(
                name: "IX_SysPolicy_CreatedBy",
                table: "SysPolicy");

            migrationBuilder.DropIndex(
                name: "IX_SysPolicy_UpdatedBy",
                table: "SysPolicy");
        }
    }
}
