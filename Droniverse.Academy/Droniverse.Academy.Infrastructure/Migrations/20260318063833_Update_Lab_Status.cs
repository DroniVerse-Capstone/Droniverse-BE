using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Lab_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Lab");

            migrationBuilder.AddColumn<sbyte>(
                name: "Status",
                table: "Lab",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab",
                sql: "`Status` IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lab");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Lab",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
