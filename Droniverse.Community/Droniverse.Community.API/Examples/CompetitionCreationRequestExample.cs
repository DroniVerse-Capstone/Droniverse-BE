using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.Helpers;
using Droniverse.Shared.Services;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionCreationRequestExample : IMultipleExamplesProvider<CompetitionCreationRequest>
    {
        public IEnumerable<SwaggerExample<CompetitionCreationRequest>> GetExamples()
        {
            var now = new ClockService().Now.TrimToMinute();

            // 01. Standard Competition
            yield return SwaggerExample.Create(
                "01. Standard - Basic Competition",
                new CompetitionCreationRequest
                {
                    NameVN = "Cuộc thi lập trình Drone 2024",
                    NameEN = "Drone Programming Competition 2024",
                    DescriptionVN = "Cuộc thi lập trình điều khiển drone dành cho sinh viên",
                    DescriptionEN = "Drone programming competition for students",
                    RuleContent = "Quy định cuộc thi...",
                    MaxParticipants = 100,

                    VisibleAt = DateTime.Parse("2023-12-25T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-09T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-01-10T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-20T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-01-21T00:00:00Z"),

                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // 02. Limited Participants
            yield return SwaggerExample.Create(
                "02. Participants - Limited",
                new CompetitionCreationRequest
                {
                    NameVN = "Giải đấu Drone Racing 2024",
                    NameEN = "Drone Racing Championship 2024",
                    DescriptionVN = "Giới hạn 50 người",
                    DescriptionEN = "Limited to 50 participants",
                    RuleContent = "Rules...",
                    MaxParticipants = 50,

                    VisibleAt = DateTime.Parse("2024-01-25T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2024-02-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-02-10T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-02-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-02-28T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-03-01T00:00:00Z"),

                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // 03. Unlimited Participants
            yield return SwaggerExample.Create(
                "03. Participants - Unlimited",
                new CompetitionCreationRequest
                {
                    NameVN = "Marathon lập trình Drone 2024",
                    NameEN = "Drone Programming Marathon 2024",
                    DescriptionVN = "Không giới hạn",
                    DescriptionEN = "Unlimited participants",
                    RuleContent = "Rules...",
                    MaxParticipants = null,

                    VisibleAt = DateTime.Parse("2024-02-20T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2024-03-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-03-15T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-03-20T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-03-25T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-03-26T00:00:00Z"),

                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // 04. Long-term Competition
            yield return SwaggerExample.Create(
                "04. Timeline - Long Term",
                new CompetitionCreationRequest
                {
                    NameVN = "Giải vô địch Drone Quốc gia 2024",
                    NameEN = "National Drone Championship 2024",
                    DescriptionVN = "5 rounds trong 2 tháng",
                    DescriptionEN = "5 rounds over 2 months",
                    RuleContent = "Rules...",
                    MaxParticipants = 200,

                    VisibleAt = DateTime.Parse("2024-03-20T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2024-04-01T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-04-15T23:59:59Z"),
                    StartDate = DateTime.Parse("2024-05-01T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-06-30T23:59:59Z"),
                    ResultPublishedAt = DateTime.Parse("2024-07-01T00:00:00Z"),

                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );

            // 05. Quick Test
            yield return SwaggerExample.Create(
                "05. Test - Quick Timeline",
                @"VisibleAt +2m → Registration start +4m → Registration end +6m → Start +8m → End +10m → Result +15m",
                CreateQuickTest(now)
            );

            // 06. Realistic Timeline
            yield return SwaggerExample.Create(
                "06. Test - Realistic Timeline",
                @"VisibleAt +5m → Registration start +10m → Registration end +30m → Start +1h → End +2h → Result +3h",
                CreateRealisticTest(now)
            );

            // 07. Error Example
            yield return SwaggerExample.Create(
                "07. Error - Invalid Registration Period",
                new CompetitionCreationRequest
                {
                    NameVN = "Sai thời gian đăng ký",
                    NameEN = "Invalid registration period",
                    DescriptionVN = "Test validation",
                    DescriptionEN = "Test validation",
                    RuleContent = "Rules...",
                    MaxParticipants = 100,

                    VisibleAt = DateTime.Parse("2024-01-01T00:00:00Z"),
                    RegistrationStartDate = DateTime.Parse("2024-01-10T00:00:00Z"),
                    RegistrationEndDate = DateTime.Parse("2024-01-05T00:00:00Z"), // ❌ sai
                    StartDate = DateTime.Parse("2024-01-15T00:00:00Z"),
                    EndDate = DateTime.Parse("2024-01-20T00:00:00Z"),
                    ResultPublishedAt = DateTime.Parse("2024-01-21T00:00:00Z"),

                    ClubID = Guid.Parse("d1822ac3-00ac-46db-9b74-3b4df9621765")
                }
            );
        }

        private CompetitionCreationRequest CreateQuickTest(DateTime now)
        {
            return new CompetitionCreationRequest
            {
                NameVN = "Quick Test",
                NameEN = "Quick Test",
                DescriptionVN = "Test nhanh",
                DescriptionEN = "Quick test",
                RuleContent = "Rules...",
                MaxParticipants = 50,

                VisibleAt = now.AddMinutes(2),
                RegistrationStartDate = now.AddMinutes(4),
                RegistrationEndDate = now.AddMinutes(6),
                StartDate = now.AddMinutes(8),
                EndDate = now.AddMinutes(10),
                ResultPublishedAt = now.AddMinutes(15),

                ClubID = Guid.Parse("7afe343c-77f0-4add-9660-d2417459445c")
            };
        }

        private CompetitionCreationRequest CreateRealisticTest(DateTime now)
        {
            return new CompetitionCreationRequest
            {
                NameVN = "Realistic Timeline",
                NameEN = "Realistic Timeline",
                DescriptionVN = "Demo",
                DescriptionEN = "Demo",
                RuleContent = "Rules...",
                MaxParticipants = 150,

                VisibleAt = now.AddMinutes(5),
                RegistrationStartDate = now.AddMinutes(10),
                RegistrationEndDate = now.AddMinutes(30),
                StartDate = now.AddHours(1),
                EndDate = now.AddHours(2),
                ResultPublishedAt = now.AddHours(3),

                ClubID = Guid.Parse("7afe343c-77f0-4add-9660-d2417459445c")
            };
        }
    }
}