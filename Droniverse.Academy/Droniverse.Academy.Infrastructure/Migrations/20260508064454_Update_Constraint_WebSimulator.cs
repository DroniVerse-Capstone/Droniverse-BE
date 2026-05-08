using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Constraint_WebSimulator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator",
                sql: "`Type` IN ('PHYSIC', 'LAB_PHYSIC', 'REAL_PHYSIC')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator");

            migrationBuilder.AddCheckConstraint(
                name: "CK_WebSimulator_Type",
                table: "WebSimulator",
                sql: "`Type` IN ('PHYSIC', 'LAB_PHYSIC')");
        }
    }
}
