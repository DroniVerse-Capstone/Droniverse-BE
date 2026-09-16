using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_CompetitionLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitionLevel",
                columns: table => new
                {
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LevelID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionLevel", x => new { x.CompetitionID, x.LevelID });
                    table.ForeignKey(
                        name: "FK_CompetitionLevel_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionLevel_LevelID",
                table: "CompetitionLevel",
                column: "LevelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionLevel");
        }
    }
}
