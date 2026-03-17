using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Quiz_Lesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `QuizAnswer`;");

            migrationBuilder.Sql("DROP TABLE IF EXISTS `UserAttempt`;");

            migrationBuilder.AlterColumn<string>(
                name: "ContentVN",
                table: "QuizQuestion",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "ContentEN",
                table: "QuizQuestion",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            // Skipped legacy add-column steps because target database already contains these columns.

            // Skip create-table/check-constraint steps for partially migrated databases.

            // Skip create-index steps for partially migrated databases.

            // Skip adding Course.CurrentVersionID index/FK in this migration because
            // target database may already contain them from previous partial runs.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_CourseVersion_CurrentVersionID",
                table: "Course");

            migrationBuilder.DropTable(
                name: "QuizQuestionAttempt");

            migrationBuilder.DropTable(
                name: "UserLesson");

            migrationBuilder.DropTable(
                name: "QuizAttempt");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuizQuestion_CorrectAnswer",
                table: "QuizQuestion");

            migrationBuilder.DropIndex(
                name: "IX_Course_CurrentVersionID",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "AnswerA",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerB",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerC",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerD",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "CurrentVersionID",
                table: "Course");

            migrationBuilder.AlterColumn<string>(
                name: "ContentVN",
                table: "QuizQuestion",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ContentEN",
                table: "QuizQuestion",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "QuizQuestion",
                type: "varchar(30)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "QuizAnswer",
                columns: table => new
                {
                    AnswerID = table.Column<Guid>(type: "char(36)", nullable: false),
                    QuestionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContentVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswer", x => x.AnswerID);
                    table.ForeignKey(
                        name: "FK_QuizAnswer_QuizQuestion_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "QuizQuestion",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserAttempt",
                columns: table => new
                {
                    AttemptID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false),
                    AttemptTime = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttempt", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_UserAttempt_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lab_Type1",
                table: "QuizQuestion",
                sql: "`Type` IN ('MULTIPLE_CHOICE', 'TRUE_FALSE')");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersion_CourseID",
                table: "CourseVersion",
                column: "CourseID",
                unique: true,
                filter: "`Status` = 1");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_QuestionID",
                table: "QuizAnswer",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempt_LessonID",
                table: "UserAttempt",
                column: "LessonID");
        }
    }
}
