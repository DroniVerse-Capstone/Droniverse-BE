using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_User_Simulator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSimulator_UserLesson_UserLessonID",
                table: "UserSimulator");

            migrationBuilder.DropIndex(
                name: "IX_UserSimulator_UserLessonID",
                table: "UserSimulator");

            migrationBuilder.RenameColumn(
                name: "UserLessonID",
                table: "UserSimulator",
                newName: "UserID");

            migrationBuilder.AddColumn<Guid>(
                name: "LessonID",
                table: "UserSimulator",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserSimulator_LessonID",
                table: "UserSimulator",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimulator_UserID_LessonID",
                table: "UserSimulator",
                columns: new[] { "UserID", "LessonID" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimulator_Lesson_LessonID",
                table: "UserSimulator",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSimulator_Lesson_LessonID",
                table: "UserSimulator");

            migrationBuilder.DropIndex(
                name: "IX_UserSimulator_LessonID",
                table: "UserSimulator");

            migrationBuilder.DropIndex(
                name: "IX_UserSimulator_UserID_LessonID",
                table: "UserSimulator");

            migrationBuilder.DropColumn(
                name: "LessonID",
                table: "UserSimulator");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "UserSimulator",
                newName: "UserLessonID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimulator_UserLessonID",
                table: "UserSimulator",
                column: "UserLessonID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSimulator_UserLesson_UserLessonID",
                table: "UserSimulator",
                column: "UserLessonID",
                principalTable: "UserLesson",
                principalColumn: "UserLessonID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
