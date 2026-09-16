using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Status_Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseVersion_Status",
                table: "CourseVersion");

            migrationBuilder.AlterColumn<sbyte>(
                name: "Status",
                table: "UserCertificate",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AlterColumn<sbyte>(
                name: "Status",
                table: "CourseVersion",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit");

            migrationBuilder.AddColumn<sbyte>(
                name: "Status",
                table: "Course",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseVersion_Status",
                table: "CourseVersion",
                sql: "`Status` IN (0,1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Course_Status",
                table: "Course",
                sql: "`Status` IN (0,1,2,3,4)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CourseVersion_Status",
                table: "CourseVersion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Course_Status",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Course");

            migrationBuilder.AlterColumn<ulong>(
                name: "Status",
                table: "UserCertificate",
                type: "bit",
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<ulong>(
                name: "Status",
                table: "CourseVersion",
                type: "bit",
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "tinyint");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseVersion_Status",
                table: "CourseVersion",
                sql: "`Status` IN (0, 1)");
        }
    }
}
