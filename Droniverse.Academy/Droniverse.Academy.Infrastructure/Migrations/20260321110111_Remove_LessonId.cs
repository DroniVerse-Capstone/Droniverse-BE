using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_LessonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lab_Lesson_LessonID",
                table: "Lab");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Lesson_LessonID",
                table: "Quiz");

            migrationBuilder.DropForeignKey(
                name: "FK_Theory_Lesson_LessonID",
                table: "Theory");

            migrationBuilder.DropIndex(
                name: "IX_Theory_LessonID",
                table: "Theory");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_LessonID",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Lab_LessonID",
                table: "Lab");

            migrationBuilder.DropColumn(
                name: "LessonID",
                table: "Theory");

            migrationBuilder.DropColumn(
                name: "LessonID",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "LessonID",
                table: "Lab");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonID",
                table: "Theory",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LessonID",
                table: "Quiz",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LessonID",
                table: "Lab",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Theory_LessonID",
                table: "Theory",
                column: "LessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_LessonID",
                table: "Quiz",
                column: "LessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lab_LessonID",
                table: "Lab",
                column: "LessonID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lab_Lesson_LessonID",
                table: "Lab",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Lesson_LessonID",
                table: "Quiz",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Theory_Lesson_LessonID",
                table: "Theory",
                column: "LessonID",
                principalTable: "Lesson",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
