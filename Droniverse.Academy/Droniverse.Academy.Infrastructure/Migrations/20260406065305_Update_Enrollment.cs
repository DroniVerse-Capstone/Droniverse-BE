using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Enrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @column_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND COLUMN_NAME = 'CourseID'
                );
                SET @sql := IF(
                    @column_exists = 0,
                    'ALTER TABLE `Enrollment` ADD COLUMN `CourseID` char(36) NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000'';',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                UPDATE `Enrollment` e
                INNER JOIN `CourseVersion` cv ON e.`CourseVersionID` = cv.`CourseVersionID`
                SET e.`CourseID` = cv.`CourseID`
                WHERE e.`CourseID` = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.Sql(@"
                SET @index_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND INDEX_NAME = 'IX_Enrollment_CourseID'
                );
                SET @sql := IF(
                    @index_exists = 0,
                    'CREATE INDEX `IX_Enrollment_CourseID` ON `Enrollment` (`CourseID`);',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @fk_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND CONSTRAINT_NAME = 'FK_Enrollment_Course_CourseID'
                      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
                );
                SET @sql := IF(
                    @fk_exists = 0,
                    'ALTER TABLE `Enrollment` ADD CONSTRAINT `FK_Enrollment_Course_CourseID` FOREIGN KEY (`CourseID`) REFERENCES `Course` (`CourseID`) ON DELETE RESTRICT;',
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
                SET @fk_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND CONSTRAINT_NAME = 'FK_Enrollment_Course_CourseID'
                      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
                );
                SET @sql := IF(
                    @fk_exists > 0,
                    'ALTER TABLE `Enrollment` DROP FOREIGN KEY `FK_Enrollment_Course_CourseID`;',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @index_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND INDEX_NAME = 'IX_Enrollment_CourseID'
                );
                SET @sql := IF(
                    @index_exists > 0,
                    'DROP INDEX `IX_Enrollment_CourseID` ON `Enrollment`;',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @column_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Enrollment'
                      AND COLUMN_NAME = 'CourseID'
                );
                SET @sql := IF(
                    @column_exists > 0,
                    'ALTER TABLE `Enrollment` DROP COLUMN `CourseID`;',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
