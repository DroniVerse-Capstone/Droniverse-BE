using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Is_Public_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "ClubCreationRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "ClubCreationRequest",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
