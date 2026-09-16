using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_System_Policy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentEN",
                table: "SysPolicy",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentVN",
                table: "SysPolicy",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEN",
                table: "SysPolicy",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleVN",
                table: "SysPolicy",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.Sql(@"UPDATE SysPolicy
SET TitleEN = COALESCE(TitleVi, ''),
    TitleVN = COALESCE(TitleVi, ''),
    ContentEN = COALESCE(Content, ''),
    ContentVN = COALESCE(Content, '');");

            migrationBuilder.DropColumn(
                name: "TitleVi",
                table: "SysPolicy");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "SysPolicy");

            migrationBuilder.AlterColumn<string>(
                name: "ContentEN",
                table: "SysPolicy",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentVN",
                table: "SysPolicy",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleEN",
                table: "SysPolicy",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleVN",
                table: "SysPolicy",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleVi",
                table: "SysPolicy",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "SysPolicy",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE SysPolicy
SET TitleVi = COALESCE(TitleVN, TitleEN, ''),
    Content = COALESCE(ContentVN, ContentEN, '');");

            migrationBuilder.DropColumn(
                name: "ContentEN",
                table: "SysPolicy");

            migrationBuilder.DropColumn(
                name: "TitleEN",
                table: "SysPolicy");

            migrationBuilder.DropColumn(
                name: "TitleVN",
                table: "SysPolicy");

            migrationBuilder.DropColumn(
                name: "ContentVN",
                table: "SysPolicy");
        }
    }
}
