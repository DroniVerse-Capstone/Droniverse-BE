using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Type_VR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "Type",
            //    table: "VRSimulator",
            //    type: "varchar(20)",
            //    maxLength: 20,
            //    nullable: false,
            //    defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VRSimulator_Type",
                table: "VRSimulator",
                sql: "`Type` IN ('LEARNING', 'COMPETITION')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VRSimulator_Type",
                table: "VRSimulator");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "VRSimulator");
        }
    }
}
