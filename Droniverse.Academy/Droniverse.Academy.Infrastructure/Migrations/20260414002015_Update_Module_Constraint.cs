using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Module_Constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'UserModule'
      AND constraint_name = 'CK_UserModule_Progress'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists > 0,
    'ALTER TABLE `UserModule` DROP CHECK `CK_UserModule_Progress`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'Feedback'
      AND constraint_name = 'CK_Feedback_Rating'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists > 0,
    'ALTER TABLE `Feedback` DROP CHECK `CK_Feedback_Rating`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'UserCertificate'
      AND column_name = 'SerialNumber'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `UserCertificate` DROP COLUMN `SerialNumber`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Certificate'
      AND column_name = 'AuthorName'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `Certificate` DROP COLUMN `AuthorName`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Certificate'
      AND column_name = 'Description'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `Certificate` DROP COLUMN `Description`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Certificate'
      AND column_name = 'LogoCertificate'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `Certificate` DROP COLUMN `LogoCertificate`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Certificate'
      AND column_name = 'Signature'
);
SET @sql := IF(
    @column_exists > 0,
    'ALTER TABLE `Certificate` DROP COLUMN `Signature`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'UserCertificate'
      AND column_name = 'CertificateUrl'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `UserCertificate` ADD COLUMN `CertificateUrl` text NOT NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClubID",
                table: "Enrollment",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'ClubID'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `ClubID` char(36) NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000'';',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'CreatedAt'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `CreatedAt` datetime NOT NULL DEFAULT ''0001-01-01 00:00:00'';',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'CreatedBy'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `CreatedBy` char(36) NOT NULL DEFAULT ''00000000-0000-0000-0000-000000000000'';',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'OwnedUserID'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `OwnedUserID` char(36) NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'UpdatedAt'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `UpdatedAt` datetime NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'UpdatedBy'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `UpdatedBy` char(36) NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'UsedByUserID'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `UsedByUserID` char(36) NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @column_exists := (
    SELECT COUNT(*)
    FROM information_schema.columns
    WHERE table_schema = @schema
      AND table_name = 'Code'
      AND column_name = 'UsedDate'
);
SET @sql := IF(
    @column_exists = 0,
    'ALTER TABLE `Code` ADD COLUMN `UsedDate` datetime NULL;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'UserModule'
      AND constraint_name = 'CK_UserModule_Progress'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists = 0,
    'ALTER TABLE `UserModule` ADD CONSTRAINT `CK_UserModule_Progress` CHECK (`Progress` >= 0 AND `Progress` <= 100);',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'Feedback'
      AND constraint_name = 'CK_Feedback_Rating'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists = 0,
    'ALTER TABLE `Feedback` ADD CONSTRAINT `CK_Feedback_Rating` CHECK (`Rating` BETWEEN 1 AND 5);',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'Code'
      AND constraint_name = 'CK_Code_Status'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists = 0,
    'ALTER TABLE `Code` ADD CONSTRAINT `CK_Code_Status` CHECK (`Status` IN (1,2,3,4));',
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
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'UserModule'
      AND constraint_name = 'CK_UserModule_Progress'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists > 0,
    'ALTER TABLE `UserModule` DROP CHECK `CK_UserModule_Progress`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'Feedback'
      AND constraint_name = 'CK_Feedback_Rating'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists > 0,
    'ALTER TABLE `Feedback` DROP CHECK `CK_Feedback_Rating`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @schema := DATABASE();
SET @constraint_exists := (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema
      AND table_name = 'Code'
      AND constraint_name = 'CK_Code_Status'
      AND constraint_type = 'CHECK'
);
SET @sql := IF(
    @constraint_exists > 0,
    'ALTER TABLE `Code` DROP CHECK `CK_Code_Status`;',
    'SELECT 1;'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "UserCertificate");

            migrationBuilder.DropColumn(
                name: "ClubID",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "OwnedUserID",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "UsedByUserID",
                table: "Code");

            migrationBuilder.DropColumn(
                name: "UsedDate",
                table: "Code");

            migrationBuilder.AddColumn<Guid>(
                name: "SerialNumber",
                table: "UserCertificate",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ClubID",
                table: "Enrollment",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Certificate",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Certificate",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoCertificate",
                table: "Certificate",
                type: "text",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "Certificate",
                type: "text",
                nullable: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserModule_Progress",
                table: "UserModule",
                sql: "`Progress` IN (0, 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Feedback_Rating",
                table: "Feedback",
                sql: "`Rating` IN (1, 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Code_Status",
                table: "Code",
                sql: "`Status` IN (0, 1)");
        }
    }
}
