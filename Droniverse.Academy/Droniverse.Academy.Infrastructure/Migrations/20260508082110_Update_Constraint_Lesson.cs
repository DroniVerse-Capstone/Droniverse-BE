using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Constraint_Lesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR', 'ASSIGNMENT', 'REAL_PHYSIC')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR', 'ASSIGNMENT')");
        }
    }
}
