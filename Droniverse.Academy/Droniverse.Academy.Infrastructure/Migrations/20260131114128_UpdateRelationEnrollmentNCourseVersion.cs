using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationEnrollmentNCourseVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseVersionID",
                table: "Enrollment",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "Enrollment",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseVersionID",
                table: "Enrollment",
                column: "CourseVersionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_CourseVersion_CourseVersionID",
                table: "Enrollment",
                column: "CourseVersionID",
                principalTable: "CourseVersion",
                principalColumn: "CourseVersionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_CourseVersion_CourseVersionID",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CourseVersionID",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CourseVersionID",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "Enrollment");
        }
    }
}
