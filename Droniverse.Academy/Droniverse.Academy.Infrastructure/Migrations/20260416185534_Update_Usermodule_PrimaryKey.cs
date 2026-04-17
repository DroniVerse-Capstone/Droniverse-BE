using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Usermodule_PrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModule",
                table: "UserModule");

            migrationBuilder.AddColumn<Guid>(
                name: "UserModuleID",
                table: "UserModule",
                type: "char(36)",
                nullable: true);

            migrationBuilder.Sql("UPDATE `UserModule` SET `UserModuleID` = UUID() WHERE `UserModuleID` IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserModuleID",
                table: "UserModule",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModule",
                table: "UserModule",
                column: "UserModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule",
                columns: new[] { "ModuleID", "UserID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModule",
                table: "UserModule");

            migrationBuilder.DropIndex(
                name: "IX_UserModule_ModuleID_UserID",
                table: "UserModule");

            migrationBuilder.DropColumn(
                name: "UserModuleID",
                table: "UserModule");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModule",
                table: "UserModule",
                columns: new[] { "ModuleID", "UserID" });
        }
    }
}
