using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Big_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CourseVersionCategory");

            //migrationBuilder.DropTable(
            //    name: "RequiredDrone");

            //migrationBuilder.DropCheckConstraint(
            //    name: "CK_Lesson_Type",
            //    table: "Lesson");

            //migrationBuilder.DropCheckConstraint(
            //    name: "CK_CourseVersion_Level",
            //    table: "CourseVersion");

            //migrationBuilder.DropColumn(
            //    name: "Level",
            //    table: "CourseVersion");

            //migrationBuilder.DropColumn(
            //    name: "OwnedUserID",
            //    table: "Code");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedAt",
            //    table: "Code");

            //migrationBuilder.DropColumn(
            //    name: "UpdatedBy",
            //    table: "Code");
            //migrationBuilder.AlterColumn<string>(
            //    name: "Model3DLink",
            //    table: "Drone",
            //    type: "char(255)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "text");

            //migrationBuilder.RenameColumn(
            //    name: "Model3DLink",
            //    table: "Drone",
            //    newName: "ImgURL");

            migrationBuilder.AddColumn<Guid>(
                name: "DroneID",
                table: "Course",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelID",
                table: "Course",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlightSimulator",
                columns: table => new
                {
                    FlightID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "text", nullable: true),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSimulator", x => x.FlightID);
                    table.CheckConstraint("CK_FlightSimulator_EstimatedTime", "`EstimatedTime` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Level",
                columns: table => new
                {
                    LevelID = table.Column<Guid>(type: "char(36)", nullable: false),
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Level", x => x.LevelID);
                    table.CheckConstraint("CK_Level_LevelNumber", "`LevelNumber` IN (1,2,3,4)");
                    table.ForeignKey(
                        name: "FK_Level_Drone_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drone",
                        principalColumn: "DroneID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PrerequisiteCourse",
                columns: table => new
                {
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    PrerequisiteCourseID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrerequisiteCourse", x => new { x.CourseID, x.PrerequisiteCourseID });
                    table.CheckConstraint("CK_PrerequisiteCourse_SelfReference", "`CourseID` <> `PrerequisiteCourseID`");
                    table.ForeignKey(
                        name: "FK_PrerequisiteCourse_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrerequisiteCourse_Course_PrerequisiteCourseID",
                        column: x => x.PrerequisiteCourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StructureSimulator",
                columns: table => new
                {
                    StructureID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "text", nullable: true),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureSimulator", x => x.StructureID);
                    table.CheckConstraint("CK_StructureSimulator_EstimatedTime", "`EstimatedTime` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LevelCourseRequirement",
                columns: table => new
                {
                    LevelID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelCourseRequirement", x => new { x.LevelID, x.CourseID });
                    table.ForeignKey(
                        name: "FK_LevelCourseRequirement_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LevelCourseRequirement_Level_LevelID",
                        column: x => x.LevelID,
                        principalTable: "Level",
                        principalColumn: "LevelID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserLevel",
                columns: table => new
                {
                    UserLevelID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LevelID = table.Column<Guid>(type: "char(36)", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLevel", x => x.UserLevelID);
                    table.ForeignKey(
                        name: "FK_UserLevel_Level_LevelID",
                        column: x => x.LevelID,
                        principalTable: "Level",
                        principalColumn: "LevelID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'STRUCTURE_SIMULATOR', 'FLIGHT_SIMULATOR')");

            migrationBuilder.CreateIndex(
                name: "IX_Course_DroneID",
                table: "Course",
                column: "DroneID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_LevelID",
                table: "Course",
                column: "LevelID");

            migrationBuilder.CreateIndex(
                name: "IX_Level_DroneID_LevelNumber",
                table: "Level",
                columns: new[] { "DroneID", "LevelNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevelCourseRequirement_CourseID",
                table: "LevelCourseRequirement",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_PrerequisiteCourse_PrerequisiteCourseID",
                table: "PrerequisiteCourse",
                column: "PrerequisiteCourseID");

            migrationBuilder.CreateIndex(
                name: "IX_UserLevel_LevelID",
                table: "UserLevel",
                column: "LevelID");

            migrationBuilder.CreateIndex(
                name: "IX_UserLevel_UserID_LevelID",
                table: "UserLevel",
                columns: new[] { "UserID", "LevelID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Drone_DroneID",
                table: "Course",
                column: "DroneID",
                principalTable: "Drone",
                principalColumn: "DroneID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Level_LevelID",
                table: "Course",
                column: "LevelID",
                principalTable: "Level",
                principalColumn: "LevelID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Drone_DroneID",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Level_LevelID",
                table: "Course");

            migrationBuilder.DropTable(
                name: "FlightSimulator");

            migrationBuilder.DropTable(
                name: "LevelCourseRequirement");

            migrationBuilder.DropTable(
                name: "PrerequisiteCourse");

            migrationBuilder.DropTable(
                name: "StructureSimulator");

            migrationBuilder.DropTable(
                name: "UserLevel");

            migrationBuilder.DropTable(
                name: "Level");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.DropIndex(
                name: "IX_Course_DroneID",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Course_LevelID",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "DroneID",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "LevelID",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "ImgURL",
                table: "Drone",
                newName: "Model3DLink");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "CourseVersion",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EASY");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnedUserID",
                table: "Code",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Code",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Code",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodeUsage",
                columns: table => new
                {
                    CodeID = table.Column<string>(type: "varchar(50)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeUsage", x => new { x.CodeID, x.UserID });
                    table.ForeignKey(
                        name: "FK_CodeUsage_Code_CodeID",
                        column: x => x.CodeID,
                        principalTable: "Code",
                        principalColumn: "CodeID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CourseVersionCategory",
                columns: table => new
                {
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersionCategory", x => new { x.CourseVersionID, x.CategoryID });
                    table.ForeignKey(
                        name: "FK_CourseVersionCategory_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequiredDrone",
                columns: table => new
                {
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredDrone", x => new { x.CourseVersionID, x.DroneID });
                    table.ForeignKey(
                        name: "FK_RequiredDrone_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequiredDrone_Drone_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drone",
                        principalColumn: "DroneID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CourseVersion_Level",
                table: "CourseVersion",
                sql: "`Level` IN ('EASY','MEDIUM','HARD')");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDrone_DroneID",
                table: "RequiredDrone",
                column: "DroneID");
        }
    }
}
