using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_Simulator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSimulator",
                columns: table => new
                {
                    UserSimulatorID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserLessonID = table.Column<Guid>(type: "char(36)", nullable: false),
                    FlightTime = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    IsSuccess = table.Column<ulong>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSimulator", x => x.UserSimulatorID);
                    table.ForeignKey(
                        name: "FK_UserSimulator_UserLesson_UserLessonID",
                        column: x => x.UserLessonID,
                        principalTable: "UserLesson",
                        principalColumn: "UserLessonID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserSimulator_UserLessonID",
                table: "UserSimulator",
                column: "UserLessonID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSimulator");
        }
    }
}
