using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Assignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.CreateTable(
                name: "Assignment",
                columns: table => new
                {
                    AssignmentID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleEN = table.Column<string>(type: "text", nullable: false),
                    TitleVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    Requirement = table.Column<string>(type: "text", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.AssignmentID);
                    table.CheckConstraint("CK_Assignment_EstimatedTime", "`EstimatedTime` >= 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserAssignment",
                columns: table => new
                {
                    UserAssignmentID = table.Column<Guid>(type: "char(36)", nullable: false),
                    AssignmentID = table.Column<Guid>(type: "char(36)", nullable: false),
                    EnrollmentID = table.Column<Guid>(type: "char(36)", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    MediaID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false, defaultValue: (sbyte)0),
                    Score = table.Column<int>(type: "int", nullable: true),
                    ReviewComment = table.Column<string>(type: "text", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssignment", x => x.UserAssignmentID);
                    table.CheckConstraint("CK_UserAssignment_AttemptNumber", "`AttemptNumber` >= 1");
                    table.CheckConstraint("CK_UserAssignment_ReviewRequiredOnFinalStatus", "(`Status` IN (2,3) AND `ReviewedBy` IS NOT NULL AND `ReviewedAt` IS NOT NULL AND `Score` IS NOT NULL) OR (`Status` IN (0,1))");
                    table.CheckConstraint("CK_UserAssignment_Score", "`Score` IS NULL OR (`Score` BETWEEN 0 AND 100)");
                    table.CheckConstraint("CK_UserAssignment_Status", "`Status` IN (0,1,2,3)");
                    table.ForeignKey(
                        name: "FK_UserAssignment_Assignment_AssignmentID",
                        column: x => x.AssignmentID,
                        principalTable: "Assignment",
                        principalColumn: "AssignmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssignment_Enrollment_EnrollmentID",
                        column: x => x.EnrollmentID,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR', 'ASSIGNMENT')");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignment_AssignmentID_EnrollmentID_AttemptNumber",
                table: "UserAssignment",
                columns: new[] { "AssignmentID", "EnrollmentID", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignment_EnrollmentID_Status_SubmittedAt",
                table: "UserAssignment",
                columns: new[] { "EnrollmentID", "Status", "SubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAssignment");

            migrationBuilder.DropTable(
                name: "Assignment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR')");
        }
    }
}
