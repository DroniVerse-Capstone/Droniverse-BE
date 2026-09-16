using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_New_Lesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Report_Lab_LabID",
                table: "Report");

            migrationBuilder.DropTable(
                name: "FlightSimulator");

            migrationBuilder.DropTable(
                name: "StructureSimulator");

            migrationBuilder.DropIndex(
                name: "IX_Report_LabID",
                table: "Report");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.RenameColumn(
                name: "LabID",
                table: "Report",
                newName: "ReferenceID");

            migrationBuilder.CreateTable(
                name: "VRSimulator",
                columns: table => new
                {
                    VRSimulatorID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleEN = table.Column<string>(type: "text", nullable: false),
                    TitleVN = table.Column<string>(type: "text", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VRSimulator", x => x.VRSimulatorID);
                    table.CheckConstraint("CK_VRSimulator_EstimatedTime", "`EstimatedTime` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebSimulator",
                columns: table => new
                {
                    WebSimulatorID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleEN = table.Column<string>(type: "text", nullable: false),
                    TitleVN = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSimulator", x => x.WebSimulatorID);
                    table.CheckConstraint("CK_WebSimulator_EstimatedTime", "`EstimatedTime` > 0");
                    table.CheckConstraint("CK_WebSimulator_Type", "`Type` IN ('Physic', 'LabPhysic')");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'WEB', 'VR')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VRSimulator");

            migrationBuilder.DropTable(
                name: "WebSimulator");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson");

            migrationBuilder.RenameColumn(
                name: "ReferenceID",
                table: "Report",
                newName: "LabID");

            migrationBuilder.CreateTable(
                name: "FlightSimulator",
                columns: table => new
                {
                    FlightID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSimulator", x => x.FlightID);
                    table.CheckConstraint("CK_FlightSimulator_EstimatedTime", "`EstimatedTime` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StructureSimulator",
                columns: table => new
                {
                    StructureID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureSimulator", x => x.StructureID);
                    table.CheckConstraint("CK_StructureSimulator_EstimatedTime", "`EstimatedTime` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Report_LabID",
                table: "Report",
                column: "LabID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lesson_Type",
                table: "Lesson",
                sql: "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'STRUCTURE_SIMULATOR', 'FLIGHT_SIMULATOR')");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Lab_LabID",
                table: "Report",
                column: "LabID",
                principalTable: "Lab",
                principalColumn: "LabID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
