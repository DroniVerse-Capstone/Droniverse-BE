using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class RoundUpdateExample : IMultipleExamplesProvider<RoundUpdateDto>
    {
        public IEnumerable<SwaggerExample<RoundUpdateDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Update - Valid",
                new RoundUpdateDto
                {
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-999999999999"),
                    RoundNumber = 1,
                    StartTime = DateTime.Parse("2024-01-11T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-11T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Update - Change Lab Only",
                new RoundUpdateDto
                {
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-888888888888"),
                    RoundNumber = 1,
                    StartTime = DateTime.Parse("2024-01-10T09:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-10T11:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Update - Change Time Only",
                new RoundUpdateDto
                {
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-111111111111"),
                    RoundNumber = 1,
                    StartTime = DateTime.Parse("2024-01-11T14:00:00Z"),
                    EndTime = DateTime.Parse("2024-01-11T16:00:00Z")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Overlap with Round 2",
                new RoundUpdateDto
                {
                    LabID = Guid.Parse("a1b2c3d4-e5f6-4789-a012-777777777777"),
                    RoundNumber = 1,
                    StartTime = DateTime.Parse("2024-01-12T10:00:00Z"), // Overlap v?i Round 2 (09:00-11:00)
                    EndTime = DateTime.Parse("2024-01-12T12:00:00Z")
                }
            );
        }
    }
}
