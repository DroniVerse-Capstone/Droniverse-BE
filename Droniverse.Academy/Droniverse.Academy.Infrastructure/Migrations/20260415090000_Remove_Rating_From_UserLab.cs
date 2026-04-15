using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Rating_From_UserLab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'UserLab'
      AND column_name = 'Rating'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `UserLab` DROP COLUMN `Rating`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'UserLab'
      AND column_name = 'Rating'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `UserLab` ADD COLUMN `Rating` int NOT NULL DEFAULT 0;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}
