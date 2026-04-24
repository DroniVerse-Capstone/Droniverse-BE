using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedbackEN",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "FeedbackVN",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "IsSequentialCheckpoints",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "NumberOfSteps",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "PathLength",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "UserRound");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "UserRound");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedbackEN",
                table: "UserRound",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackVN",
                table: "UserRound",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSequentialCheckpoints",
                table: "UserRound",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfSteps",
                table: "UserRound",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PathLength",
                table: "UserRound",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "UserRound",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "UserRound",
                type: "text",
                nullable: true);
        }
    }
}
