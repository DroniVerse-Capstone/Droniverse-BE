using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    public partial class Update_Course_Version_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove old check constraint
            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseVersion_Level",
                table: "CourseVersion");

            // Alter nullable fields
            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                table: "CourseVersion",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateAt",
                table: "CourseVersion",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CourseVersion",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedDuration",
                table: "CourseVersion",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionVN",
                table: "CourseVersion",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEN",
                table: "CourseVersion",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // Add new columns
            migrationBuilder.AddColumn<string>(
                name: "ContextEN",
                table: "CourseVersion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextVN",
                table: "CourseVersion",
                type: "text",
                nullable: true);

            // Composite unique index (CourseID + Version)
            migrationBuilder.CreateIndex(
                name: "IX_CourseVersion_CourseID_Version",
                table: "CourseVersion",
                columns: new[] { "CourseID", "Version" },
                unique: true);

            // Add check constraint again
            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseVersion_Level",
                table: "CourseVersion",
                sql: "`Level` IN ('EASY','MEDIUM','HARD')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseVersion_CourseID_Version",
                table: "CourseVersion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseVersion_Level",
                table: "CourseVersion");

            migrationBuilder.DropColumn(
                name: "ContextEN",
                table: "CourseVersion");

            migrationBuilder.DropColumn(
                name: "ContextVN",
                table: "CourseVersion");

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdateBy",
                table: "CourseVersion",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateAt",
                table: "CourseVersion",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CourseVersion",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstimatedDuration",
                table: "CourseVersion",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionVN",
                table: "CourseVersion",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEN",
                table: "CourseVersion",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseVersion_Level",
                table: "CourseVersion",
                sql: "`Level` IN ('EASY','MEDIUM','HARD')");
        }
    }
}