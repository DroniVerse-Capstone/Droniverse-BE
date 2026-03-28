using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionUpdateExample : IMultipleExamplesProvider<CompetitionUpdateDto>
    {
        public IEnumerable<SwaggerExample<CompetitionUpdateDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "1. Update - Valid (DRAFT status, full timeline update)",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi lập trình Drone 2026 (Cập nhật)",
                    NameEN = "Drone Programming Competition 2026 (Updated)",
                    DescriptionVN = "Cuộc thi dành cho sinh viên - cập nhật thông tin đầy đủ ở trạng thái DRAFT",
                    DescriptionEN = "Competition for students - full update while in DRAFT status",
                    RuleContent = "Quy định cuộc thi đã được cập nhật: thời gian làm bài 90 phút, nộp bài trước hạn.",
                    MaxParticipants = 150,
                    VisibleAt = DateTime.Parse("2026-01-01T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2026-01-05T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2026-01-12T23:59:59Z"),
                    StartDate = DateTime.Parse("2026-01-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2026-01-25T23:59:59Z")
                }
            );

            yield return SwaggerExample.Create(
                "2. Update - Valid (DRAFT, timeline changed -> rounds can become SCHEDULE_INVALID)",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi lập trình Drone 2026",
                    NameEN = "Drone Programming Competition 2026",
                    DescriptionVN = "Rút ngắn thời gian cuộc thi ở trạng thái DRAFT",
                    DescriptionEN = "Shorten competition period while in DRAFT",
                    RuleContent = "Quy định cuộc thi...",
                    MaxParticipants = 100,
                    VisibleAt = DateTime.Parse("2026-01-01T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2026-01-03T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2026-01-08T23:59:59Z"),
                    StartDate = DateTime.Parse("2026-01-10T00:00:00Z"),
                    EndDate = DateTime.Parse("2026-01-18T23:59:59Z")
                }
            );

            yield return SwaggerExample.Create(
                "3. Error - Update when REGISTRATION_CLOSED",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi đã đóng đăng ký",
                    NameEN = "Registration Closed Competition",
                    DescriptionVN = "Cố gắng update khi status = REGISTRATION_CLOSED",
                    DescriptionEN = "Try to update when status = REGISTRATION_CLOSED",
                    RuleContent = "Rules...",
                    MaxParticipants = 100,
                    VisibleAt = DateTime.Parse("2026-01-01T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2026-01-03T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2026-01-08T23:59:59Z"),
                    StartDate = DateTime.Parse("2026-01-10T00:00:00Z"),
                    EndDate = DateTime.Parse("2026-01-20T23:59:59Z")
                }
            );
        }
    }
}
