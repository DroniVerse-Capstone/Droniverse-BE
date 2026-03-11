using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionUpdateExample : IMultipleExamplesProvider<CompetitionUpdateDto>
    {
        public IEnumerable<SwaggerExample<CompetitionUpdateDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Update - Valid (DRAFT status)",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi lập trình Drone 2024 (Cập nhật)",
                    NameEN = "Drone Programming Competition 2024 (Updated)",
                    DescriptionVN = "Cuộc thi lập trình điều khiển drone dành cho sinh viên - Đã cập nhật",
                    DescriptionEN = "Drone programming competition for students - Updated",
                    RuleContent = "Quy định cuộc thi đã được cập nhật: 1. Thời gian làm bài 90 phút...",
                    MaxParticipants = 150,
                    RegistrationStartDate = DateTime.Parse("2024-01-05T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-12T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-25T23:59:59Z")
                }
            );

            yield return SwaggerExample.Create(
                "Update Time - Rounds become SCHEDULE_INVALID",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi lập trình Drone 2024",
                    NameEN = "Drone Programming Competition 2024",
                    DescriptionVN = "Rút ngắn thời gian cuộc thi",
                    DescriptionEN = "Shorten competition period",
                    RuleContent = "Quy định cuộc thi...",
                    MaxParticipants = 100,
                    RegistrationStartDate = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-07T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-10T00:00:00Z"), // Thay đổi
                    EndDate = DateTime.Parse("2024-01-18T23:59:59Z")  // Rút ngắn - Rounds sau ngày 18 sẽ bị SCHEDULE_INVALID
                }
            );

            yield return SwaggerExample.Create(
                "Error - Update when CLOSED",
                new CompetitionUpdateDto
                {
                    NameVN = "Cuộc thi đã CLOSED",
                    NameEN = "Closed Competition",
                    DescriptionVN = "Cố gắng update khi status = CLOSED",
                    DescriptionEN = "Try to update when status = CLOSED",
                    RuleContent = "Rules...",
                    MaxParticipants = 100,
                    RegistrationStartDate = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-07T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-10T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-20T23:59:59Z")
                }
            );
        }
    }
}
