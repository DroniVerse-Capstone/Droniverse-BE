using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_ModuleID_From_Lab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lab_Module_ModuleID",
                table: "Lab");

            migrationBuilder.DropIndex(
                name: "IX_Lab_ModuleID",
                table: "Lab");

            migrationBuilder.DropColumn(
                name: "ModuleID",
                table: "Lab");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModuleID",
                table: "Lab",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lab_ModuleID",
                table: "Lab",
                column: "ModuleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Lab_Module_ModuleID",
                table: "Lab",
                column: "ModuleID",
                principalTable: "Module",
                principalColumn: "ModuleID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
