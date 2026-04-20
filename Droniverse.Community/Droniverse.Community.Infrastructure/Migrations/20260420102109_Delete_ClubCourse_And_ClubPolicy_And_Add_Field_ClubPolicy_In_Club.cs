using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Delete_ClubCourse_And_ClubPolicy_And_Add_Field_ClubPolicy_In_Club : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClubCreationRequest_ClubPolicy_ClubPolicyID",
                table: "ClubCreationRequest");

            migrationBuilder.DropTable(
                name: "ClubCourse");

            migrationBuilder.DropTable(
                name: "ClubPolicy");

            migrationBuilder.DropIndex(
                name: "IX_ClubCreationRequest_ClubPolicyID",
                table: "ClubCreationRequest");

            migrationBuilder.AddColumn<string>(
                name: "ClubPolicy",
                table: "ClubCreationRequest",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DroneID",
                table: "ClubCreationRequest",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ClubPolicy",
                table: "Club",
                type: "text",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClubPolicy",
                table: "ClubCreationRequest");

            migrationBuilder.DropColumn(
                name: "DroneID",
                table: "ClubCreationRequest");

            migrationBuilder.DropColumn(
                name: "ClubPolicy",
                table: "Club");

            migrationBuilder.CreateTable(
                name: "ClubCourse",
                columns: table => new
                {
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProfitType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RemainingQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubCourse", x => new { x.ClubID, x.CourseID });
                    table.ForeignKey(
                        name: "FK_ClubCourse_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubPolicy",
                columns: table => new
                {
                    ClubPolicyID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPolicy", x => x.ClubPolicyID);
                    table.ForeignKey(
                        name: "FK_ClubPolicy_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ClubCreationRequest_ClubPolicyID",
                table: "ClubCreationRequest",
                column: "ClubPolicyID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPolicy_ClubID",
                table: "ClubPolicy",
                column: "ClubID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubCreationRequest_ClubPolicy_ClubPolicyID",
                table: "ClubCreationRequest",
                column: "ClubPolicyID",
                principalTable: "ClubPolicy",
                principalColumn: "ClubPolicyID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
