using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Drone_WebSimulator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<Guid>(
            //    name: "DroneID",
            //    table: "WebSimulator",
            //    type: "char(36)",
            //    nullable: false,
            //    defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //migrationBuilder.CreateIndex(
            //    name: "IX_WebSimulator_DroneID",
            //    table: "WebSimulator",
            //    column: "DroneID");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_WebSimulator_Drone_DroneID",
            //    table: "WebSimulator",
            //    column: "DroneID",
            //    principalTable: "Drone",
            //    principalColumn: "DroneID",
            //    onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebSimulator_Drone_DroneID",
                table: "WebSimulator");

            migrationBuilder.DropIndex(
                name: "IX_WebSimulator_DroneID",
                table: "WebSimulator");

            migrationBuilder.DropColumn(
                name: "DroneID",
                table: "WebSimulator");
        }
    }
}
