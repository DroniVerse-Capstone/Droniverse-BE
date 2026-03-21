using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Status_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "UserLesson");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "QuizAttempt");

            migrationBuilder.AddColumn<sbyte>(
                name: "Status",
                table: "UserLesson",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserLesson_Status",
                table: "UserLesson",
                sql: "`Status` IN (0,1,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab",
                sql: "`Status` IN (0, 1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment",
                sql: "`Status` IN (0,1,2,3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserLesson_Status",
                table: "UserLesson");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserLesson");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "UserLesson",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<sbyte>(
                name: "Status",
                table: "QuizAttempt",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lab_Status",
                table: "Lab",
                sql: "`Status` IN (0, 1, 2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollment_Status",
                table: "Enrollment",
                sql: "`Status` IN (0,1,2)");
        }
    }
}
