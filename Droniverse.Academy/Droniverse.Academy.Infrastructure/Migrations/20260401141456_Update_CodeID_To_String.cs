using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_CodeID_To_String : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //drop foreign key codeID trong bảng CodeUsage
            migrationBuilder.DropForeignKey(
                name: "FK_CodeUsage_Code_CodeID",
                table: "CodeUsage");

            //thay đổi kiểu dữ liệu Code trước
            migrationBuilder.AlterColumn<string>(
                name: "CodeID",
                table: "CodeUsage",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AlterColumn<string>(
                name: "CodeID",
                table: "Code",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            //tạo lại foreign key codeID trong bảng CodeUsage
            migrationBuilder.AddForeignKey(
               name: "FK_CodeUsage_Code_CodeID",
               table: "CodeUsage",
               column: "CodeID",
               principalTable: "Code",
               principalColumn: "CodeID",
               onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CodeID",
                table: "CodeUsage",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CodeID",
                table: "Code",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");
        }
    }
}
