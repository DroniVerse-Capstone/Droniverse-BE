using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class RoundCreateExample : IMultipleExamplesProvider<RoundCreateDto>
    {
        public IEnumerable<SwaggerExample<RoundCreateDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Round 1 - Valid",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-111111111111"),
                    RoundNumber = 1,
                    StartTime = DateTime.Parse("2024-01-10T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-10T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Round 2 - Valid",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-222222222222"),
                    RoundNumber = 2,
                    StartTime = DateTime.Parse("2024-01-12T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-12T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Round 3 - Valid",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-333333333333"),
                    RoundNumber = 3,
                    StartTime = DateTime.Parse("2024-01-15T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-15T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Time Overlap",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-444444444444"),
                    RoundNumber = 4,
                    StartTime = DateTime.Parse("2024-01-10T10:00:00Z"), // Trùng v?i Round 1 (09:00-11:00)
                    EndTime = DateTime.Parse("2024-01-10T12:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Duplicate Lab",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-111111111111"), // Trùng v?i Round 1
                    RoundNumber = 5,
                    StartTime = DateTime.Parse("2024-01-17T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-17T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Outside Competition Period",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-555555555555"),
                    RoundNumber = 6,
                    StartTime = DateTime.Parse("2024-01-22T09:00:00Z"), // Ngoài Competition EndDate (2024-01-20)
                    EndTime = DateTime.Parse("2024-01-22T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Duplicate RoundNumber",
                new RoundCreateDto
                {
                    CompetitionID = Guid.Parse("f5364a01-da2f-4543-a850-3cf49d14e174"),
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-666666666666"),
                    RoundNumber = 1, // Trùng v?i Round 1
                    StartTime = DateTime.Parse("2024-01-18T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-18T11:00:00Z")
                }
            );
        }
    }
}
