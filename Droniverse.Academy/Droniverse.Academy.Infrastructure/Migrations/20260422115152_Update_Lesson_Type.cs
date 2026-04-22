using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Lesson_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator",
                sql: "`Type` IN ('PHYSIC', 'LAB_PHYSIC')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator",
                sql: "`Type` IN ('Physic', 'LabPhysic')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'WEB', 'VR')");
        }
    }
}
