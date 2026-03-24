using Droniverse.Community.Application.DTO.Request;
using Droniverse.Shared.Services;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionCreationRequestExample : IMultipleExamplesProvider<CompetitionCreationRequest>
    {
        public IEnumerable<SwaggerExample<CompetitionCreationRequest>> GetExamples()
        {
            // Example 1: Competition cơ bản - tương thích với Round examples
            yield return SwaggerExample.Create(
                "Competition Standard - Compatible with Round Examples",
                new CompetitionCreationRequest
                {
                    NameVN = "Cuộc thi lập trình Drone 2024",
                    NameEN = "Drone Programming Competition 2024",
                    DescriptionVN = "Cuộc thi lập trình điều khiển drone dành cho sinh viên",
                    DescriptionEN = "Drone programming competition for students",
                    RuleContent = "Quy định cuộc thi: 1. Thời gian làm bài 60 phút. 2. Được sử dụng tài liệu. 3. Không gian lận.",
                    MaxParticipants = 100,
                    RegistrationStartDate = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-09T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-10T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-20T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-01-21T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 2: Competition với giới hạn người tham gia
            yield return SwaggerExample.Create(
                "Competition with Limited Participants",
                new CompetitionCreationRequest
                {
                    NameVN = "Giải đấu Drone Racing 2024",
                    NameEN = "Drone Racing Championship 2024",
                    DescriptionVN = "Giải đấu racing drone chuyên nghiệp - Giới hạn 50 người",
                    DescriptionEN = "Professional drone racing championship - Limited to 50 participants",
                    RuleContent = "Quy định: 1. Phải sử dụng drone do ban tổ chức cung cấp. 2. Thời gian thi đấu 10 phút/vòng.",
                    MaxParticipants = 50,
                    RegistrationStartDate = DateTime.Parse("2024-02-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-02-10T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-02-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-02-28T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-03-01T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 3: Competition không giới hạn người tham gia
            yield return SwaggerExample.Create(
                "Competition Unlimited Participants",
                new CompetitionCreationRequest
                {
                    NameVN = "Marathon lập trình Drone 2024",
                    NameEN = "Drone Programming Marathon 2024",
                    DescriptionVN = "Cuộc thi marathon 24 giờ - Không giới hạn số lượng",
                    DescriptionEN = "24-hour programming marathon - Unlimited participants",
                    RuleContent = "Quy định: 1. Thời gian 24 giờ liên tục. 2. Làm việc nhóm tối đa 3 người.",
                    MaxParticipants = null, // Không giới hạn
                    RegistrationStartDate = DateTime.Parse("2024-03-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-03-15T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-03-20T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-03-25T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-03-26T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 4: Competition dài hạn - cho phép nhiều rounds
            yield return SwaggerExample.Create(
                "Long-term Competition - Multiple Rounds",
                new CompetitionCreationRequest
                {
                    NameVN = "Giải vô địch Drone Quốc gia 2024",
                    NameEN = "National Drone Championship 2024",
                    DescriptionVN = "Giải vô địch cấp quốc gia - 5 vòng thi trong 2 tháng",
                    DescriptionEN = "National championship - 5 rounds over 2 months",
                    RuleContent = "Quy định: 1. Chia làm 5 vòng loại. 2. Mỗi vòng có bài Lab khác nhau. 3. Điểm tích lũy qua các vòng.",
                    MaxParticipants = 200,
                    RegistrationStartDate = DateTime.Parse("2024-04-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-04-15T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-05-01T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-06-30T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-07-01T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 5: Competition ngắn hạn - intensive
            yield return SwaggerExample.Create(
                "Short Intensive Competition",
                new CompetitionCreationRequest
                {
                    NameVN = "Sprint Challenge - Tuần lễ Drone",
                    NameEN = "Sprint Challenge - Drone Week",
                    DescriptionVN = "Thử thách nhanh trong 1 tuần",
                    DescriptionEN = "Fast-paced challenge in 1 week",
                    RuleContent = "Quy định: 1. Hoàn thành trong 7 ngày. 2. 3 rounds liên tiếp. 3. Điểm cao nhất thắng.",
                    MaxParticipants = 80,
                    RegistrationStartDate = DateTime.Parse("2024-05-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-05-05T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-05-06T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-05-12T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-05-13T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 6: Competition cho sinh viên mới
            yield return SwaggerExample.Create(
                "Beginner Competition",
                new CompetitionCreationRequest
                {
                    NameVN = "Cuộc thi Drone cho người mới bắt đầu",
                    NameEN = "Drone Competition for Beginners",
                    DescriptionVN = "Dành cho sinh viên mới làm quen với drone",
                    DescriptionEN = "For students new to drone programming",
                    RuleContent = "Quy định: 1. Dành cho người mới. 2. Có hướng dẫn chi tiết. 3. Mentor hỗ trợ.",
                    MaxParticipants = 150,
                    RegistrationStartDate = DateTime.Parse("2024-06-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-06-10T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-06-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-06-25T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-06-26T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            var now = new ClockService().Now;

            yield return SwaggerExample.Create(
                "Ví dụ cuộc thi với thời gian động dựa trên thời điểm hiện tại",
                "Competition With Current Time",
                @"**Quy tắc thời gian (tính từ thời điểm hiện tại):**

- `RegistrationStartDate` : 3 phút sau hiện tại
- `RegistrationEndDate` : 5 phút sau hiện tại
- `StartDate` : 7 phút sau hiện tại
- `EndDate` : 9 phút sau hiện tại
- `ResultPublishedAt` : 15 phút sau hiện tại

> Lưu ý:
> - Khoảng cách giữa các mốc thời gian rất ngắn, chỉ phù hợp cho test.
> - Nếu thời gian test đã qua. Load lại Swagger Page để lấy lại mốc thời gian mới.
> - Thời gian được tính dựa trên `ClockService().Now` theo múi giờ Việt Nam.",
                new CompetitionCreationRequest
                {
                    NameVN = "Cuộc thi với thời gian chuẩn hiện tại",
                    NameEN = "Drone Competition With Correct Time",
                    DescriptionVN = "Dành cho sinh viên mới làm quen với drone",
                    DescriptionEN = "For students new to drone programming",
                    RuleContent = "Quy định: 1. Dành cho người mới. 2. Có hướng dẫn chi tiết. 3. Mentor hỗ trợ.",
                    MaxParticipants = 150,
                    RegistrationStartDate = now.AddMinutes(3),
                    RegistrationEndDate = now.AddMinutes(5),
                    StartDate = now.AddMinutes(7),
                    EndDate = now.AddMinutes(9),
                    ResultPublishedAt = now.AddMinutes(15),
                    ClubID = Guid.Parse("7afe343c-77f0-4add-9660-d2417459445c")
                }
            );

            // Example 7: Error - Registration dates invalid
            yield return SwaggerExample.Create(
                "Error - Invalid Registration Period",
                new CompetitionCreationRequest
                {
                    NameVN = "Competition với thời gian đăng ký sai",
                    NameEN = "Competition with invalid registration period",
                    DescriptionVN = "Test validation error",
                    DescriptionEN = "Test validation error",
                    RuleContent = "Rules...",
                    MaxParticipants = 100,
                    RegistrationStartDate = DateTime.Parse("2024-01-10T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-05T23:59:59Z"), // ❌ EndDate < StartDate
                    StartDate = DateTime.Parse("2024-01-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-20T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-01-21T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // Example 8: Error - Competition dates invalid
            yield return SwaggerExample.Create(
                "Error - Invalid Competition Period",
                new CompetitionCreationRequest
                {
                    NameVN = "Competition với thời gian thi sai",
                    NameEN = "Competition with invalid competition period",
                    DescriptionVN = "Test validation error",
                    DescriptionEN = "Test validation error",
                    RuleContent = "Rules...",
                    MaxParticipants = 100,
                    RegistrationStartDate = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-09T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-20T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-15T23:59:59Z"), // ❌ EndDate < StartDate
                    ResultPublishedAt = DateTime.Parse("2024-01-21T00:00:00Z"),
                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );
        }
    }
}
