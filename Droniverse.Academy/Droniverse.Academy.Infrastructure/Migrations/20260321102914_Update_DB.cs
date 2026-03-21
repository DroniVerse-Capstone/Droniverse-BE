using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Lesson",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_ModuleID_OrderIndex",
                table: "Lesson",
                columns: new[] { "ModuleID", "OrderIndex" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_OrderIndex",
                table: "Lesson",
                sql: "`OrderIndex` > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lesson_ModuleID_OrderIndex",
                table: "Lesson");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_OrderIndex",
                table: "Lesson");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Lesson");
        }
    }
}
