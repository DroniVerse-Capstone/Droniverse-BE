using Droniverse.Community.Application.DTO.Request;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class RoundCreateExample : IMultipleExamplesProvider<RoundCreateDto>
    {
        private static readonly Guid QuickTestCompetitionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid RealisticCompetitionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly string LabId1 = "f4852f0c-309a-4ea6-8bbb-0a8fb1c0dc45";
        private static readonly string LabId2 = "495c3bba-9348-4d5d-a68e-a5f39d5300ec";

        public IEnumerable<SwaggerExample<RoundCreateDto>> GetExamples()
        {
            var now = new ClockService().Now.TrimToMinute();

            // QUICK TEST competition timeline (from CompetitionCreationRequestExample):
            // VisibleAt +2m -> RegStart +4m -> RegEnd +6m -> Start +8m -> End +10m
            // Round times should be inside [Start, End].
            yield return SwaggerExample.Create(
                "Use with a competition created by Quick Test timeline.",
                "01. Quick Test - Round 1 (Valid)",
                CreateQuickRound(now, 1, 8, 9, LabId1)
            );

            yield return SwaggerExample.Create(
                "Use with a competition created by Quick Test timeline.",
                "02. Quick Test - Round 2 (Valid)",
                CreateQuickRound(now, 2, 9, 10, LabId2)
            );

            yield return SwaggerExample.Create(
                "03. Quick Test - Error (Outside Competition Period)",
                "Round starts after Quick Test EndDate (+10m), should fail validation.",
                CreateQuickRound(now, 3, 11, 12, LabId1)
            );

            // REALISTIC competition timeline (from CompetitionCreationRequestExample):
            // VisibleAt +5m -> RegStart +10m -> RegEnd +30m -> Start +1h -> End +2h
            yield return SwaggerExample.Create(
                "04. Realistic Timeline - Round 1 (Valid)",
                "Use with a competition created by Realistic Timeline example.",
                CreateRealisticRound(now, 1, 60, 75, LabId1)
            );

            yield return SwaggerExample.Create(
                "05. Realistic Timeline - Round 2 (Valid)",
                "Use with a competition created by Realistic Timeline example.",
                CreateRealisticRound(now, 2, 90, 105, LabId2)
            );

            yield return SwaggerExample.Create(
                "06. Realistic Timeline - Error (Outside Competition Period)",
                "Round ends after Realistic EndDate (+2h), should fail validation.",
                CreateRealisticRound(now, 3, 115, 130, LabId1)
            );
        }

        private static RoundCreateDto CreateQuickRound(
            DateTime now,
            int roundNumber,
            int startOffsetMinutes,
            int endOffsetMinutes,
            string labId)
        {
            return new RoundCreateDto
            {
                CompetitionID = QuickTestCompetitionId,
                LabID = Guid.Parse(labId),
                RoundNumber = roundNumber,
                StartTime = now.AddMinutes(startOffsetMinutes),
                EndTime = now.AddMinutes(endOffsetMinutes)
            };
        }

        private static RoundCreateDto CreateRealisticRound(
            DateTime now,
            int roundNumber,
            int startOffsetMinutes,
            int endOffsetMinutes,
            string labId)
        {
            return new RoundCreateDto
            {
                CompetitionID = RealisticCompetitionId,
                LabID = Guid.Parse(labId),
                RoundNumber = roundNumber,
                StartTime = now.AddMinutes(startOffsetMinutes),
                EndTime = now.AddMinutes(endOffsetMinutes)
            };
        }
    }
}
