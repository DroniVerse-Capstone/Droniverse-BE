using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCodeToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Code_CourseVersion_CourseVersionID",
                table: "Code");

            migrationBuilder.RenameColumn(
                name: "CourseVersionID",
                table: "Code",
                newName: "CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Code_CourseVersionID",
                table: "Code",
                newName: "IX_Code_CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Code_Course_CourseID",
                table: "Code",
                column: "CourseID",
                principalTable: "Course",
                principalColumn: "CourseID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Code_Course_CourseID",
                table: "Code");

            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "Code",
                newName: "CourseVersionID");

            migrationBuilder.RenameIndex(
                name: "IX_Code_CourseID",
                table: "Code",
                newName: "IX_Code_CourseVersionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Code_CourseVersion_CourseVersionID",
                table: "Code",
                column: "CourseVersionID",
                principalTable: "CourseVersion",
                principalColumn: "CourseVersionID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
