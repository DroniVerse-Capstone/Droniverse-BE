using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Standardization_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_Course_CourseID",
                table: "Certificate");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Course_CourseID",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_Module_CourseVersion_CourseID",
                table: "Module");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CourseID",
                table: "Enrollment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Course_Status",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Certificate_CourseID",
                table: "Certificate");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "Certificate");

            migrationBuilder.RenameColumn(
                name: "CourseID",
                table: "Module",
                newName: "CourseVersionID");

            migrationBuilder.RenameIndex(
                name: "IX_Module_CourseID",
                table: "Module",
                newName: "IX_Module_CourseVersionID");

            migrationBuilder.AlterColumn<float>(
                name: "Progress",
                table: "Enrollment",
                type: "float",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateAt",
                table: "Certificate",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateAt",
                table: "Certificate",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "CertificateName",
                table: "Certificate",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Course_Status",
                table: "Course",
                sql: "`Status` IN (0,1,2,3)");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_CourseVersionID",
                table: "Certificate",
                column: "CourseVersionID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_CourseVersion_CourseVersionID",
                table: "Certificate",
                column: "CourseVersionID",
                principalTable: "CourseVersion",
                principalColumn: "CourseVersionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_CourseVersion_CourseVersionID",
                table: "Module",
                column: "CourseVersionID",
                principalTable: "CourseVersion",
                principalColumn: "CourseVersionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_CourseVersion_CourseVersionID",
                table: "Certificate");

            migrationBuilder.DropForeignKey(
                name: "FK_Module_CourseVersion_CourseVersionID",
                table: "Module");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Course_Status",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Certificate_CourseVersionID",
                table: "Certificate");

            migrationBuilder.RenameColumn(
                name: "CourseVersionID",
                table: "Module",
                newName: "CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_Module_CourseVersionID",
                table: "Module",
                newName: "IX_Module_CourseID");

            migrationBuilder.AlterColumn<float>(
                name: "Progress",
                table: "Enrollment",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float",
                oldDefaultValue: 0f);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseID",
                table: "Enrollment",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<sbyte>(
                name: "IsCompleted",
                table: "Enrollment",
                type: "tinyint",
                nullable: false,
                defaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateAt",
                table: "Certificate",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateAt",
                table: "Certificate",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "CertificateName",
                table: "Certificate",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseID",
                table: "Certificate",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseID",
                table: "Enrollment",
                column: "CourseID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Course_Status",
                table: "Course",
                sql: "`Status` IN (0,1,2,3,4)");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_CourseID",
                table: "Certificate",
                column: "CourseID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_Course_CourseID",
                table: "Certificate",
                column: "CourseID",
                principalTable: "Course",
                principalColumn: "CourseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Course_CourseID",
                table: "Enrollment",
                column: "CourseID",
                principalTable: "Course",
                principalColumn: "CourseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_CourseVersion_CourseID",
                table: "Module",
                column: "CourseID",
                principalTable: "CourseVersion",
                principalColumn: "CourseVersionID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
