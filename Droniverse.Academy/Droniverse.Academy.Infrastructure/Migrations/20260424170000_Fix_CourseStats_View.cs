using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_CourseStats_View : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW IF EXISTS `vwCourseStats`;

CREATE VIEW `vwCourseStats` AS
select
    `c`.`CourseID` AS `CourseID`,
    `cv`.`CourseVersionID` AS `CourseVersionID`,
    `cv`.`TitleVN` AS `TitleVN`,
    `cv`.`TitleEN` AS `TitleEN`,
    `cv`.`EstimatedDuration` AS `EstimatedDuration`,
    `cv`.`ImageUrl` AS `ImageUrl`,
    `cv`.`UpdateAt` AS `UpdateAt`,
    count(distinct (case when (`e`.`Status` in (1,2)) then `e`.`UserID` end)) AS `ParticipantCount`,
    avg(cast(`f`.`Rating` as decimal(5,2))) AS `AverageRating`
from (((`AcademyDB`.`Course` `c`
    join `AcademyDB`.`CourseVersion` `cv` on((`c`.`CurrentVersionID` = `cv`.`CourseVersionID`)))
    left join `AcademyDB`.`Enrollment` `e` on((`e`.`CourseVersionID` = `cv`.`CourseVersionID`)))
    left join `AcademyDB`.`Feedback` `f` on((`f`.`CourseVersionID` = `cv`.`CourseVersionID`)))
group by
    `c`.`CourseID`,
    `cv`.`CourseVersionID`,
    `cv`.`TitleVN`,
    `cv`.`TitleEN`,
    `cv`.`EstimatedDuration`,
    `cv`.`ImageUrl`,
    `cv`.`UpdateAt`;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW IF EXISTS `vwCourseStats`;
");
        }
    }
}