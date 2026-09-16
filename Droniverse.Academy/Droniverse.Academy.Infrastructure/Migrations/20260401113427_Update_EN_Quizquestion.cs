using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_EN_Quizquestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnswerA_EN",
                table: "QuizQuestion",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "AnswerB_EN",
                table: "QuizQuestion",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "AnswerC_EN",
                table: "QuizQuestion",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "AnswerD_EN",
                table: "QuizQuestion",
                type: "text",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerA_EN",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerB_EN",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerC_EN",
                table: "QuizQuestion");

            migrationBuilder.DropColumn(
                name: "AnswerD_EN",
                table: "QuizQuestion");
        }
    }
}
