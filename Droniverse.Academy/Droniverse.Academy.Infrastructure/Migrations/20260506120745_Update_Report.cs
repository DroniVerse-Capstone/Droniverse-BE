using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Report : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Report");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseVN",
                table: "Report",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseEN",
                table: "Report",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ContentEN",
                table: "Report",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentVN",
                table: "Report",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "Report",
                type: "char(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Responser",
                table: "Report",
                type: "char(36)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentEN",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "ContentVN",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "Responser",
                table: "Report");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseVN",
                table: "Report",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseEN",
                table: "Report",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Report",
                type: "text",
                nullable: false);
        }
    }
}
