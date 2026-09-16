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

        private static readonly string vrSimilatorId1 = "1d28e1b7-1ee5-4293-b27a-d1d2f2fe01d7";
        private static readonly string vrSimilatorId2 = "495c3bba-9348-4d5d-a68e-a5f39d5300ec";

        public IEnumerable<SwaggerExample<RoundCreateDto>> GetExamples()
        {
            var now = new ClockService().Now.TrimToMinute();

            // QUICK TEST
            yield return SwaggerExample.Create(
                "Use with a competition created by Quick Test timeline.",
                "01. Quick Test - Round 1 (Valid)",
                CreateQuickRound(now, 8, 9, vrSimilatorId1)
            );

            yield return SwaggerExample.Create(
                "Use with a competition created by Quick Test timeline.",
                "02. Quick Test - Round 2 (Valid)",
                CreateQuickRound(now, 9, 10, vrSimilatorId2)
            );

            yield return SwaggerExample.Create(
                "03. Quick Test - Error (Outside Competition Period)",
                "Round starts after Quick Test EndDate (+10m), should fail validation.",
                CreateQuickRound(now, 11, 12, vrSimilatorId1)
            );

            // REALISTIC
            yield return SwaggerExample.Create(
                "04. Realistic Timeline - Round 1 (Valid)",
                "Use with a competition created by Realistic Timeline example.",
                CreateRealisticRound(now, 60, 75, vrSimilatorId1)
            );

            yield return SwaggerExample.Create(
                "05. Realistic Timeline - Round 2 (Valid)",
                "Use with a competition created by Realistic Timeline example.",
                CreateRealisticRound(now, 90, 105, vrSimilatorId2)
            );

            yield return SwaggerExample.Create(
                "06. Realistic Timeline - Error (Outside Competition Period)",
                "Round ends after Realistic EndDate (+2h), should fail validation.",
                CreateRealisticRound(now, 115, 130, vrSimilatorId1)
            );
        }

        private static RoundCreateDto CreateQuickRound(
            DateTime now,
            int startOffsetMinutes,
            int endOffsetMinutes,
            string vrSimulatorId)
        {
            return new RoundCreateDto
            {
                CompetitionID = QuickTestCompetitionId,
                VRSimilatorID = Guid.Parse(vrSimulatorId),
                StartTime = now.AddMinutes(startOffsetMinutes),
                EndTime = now.AddMinutes(endOffsetMinutes),
                LimitTime = TimeSpan.FromMinutes(15)
            };
        }

        private static RoundCreateDto CreateRealisticRound(
            DateTime now,
            int startOffsetMinutes,
            int endOffsetMinutes,
            string vrSimulatorId)
        {
            return new RoundCreateDto
            {
                CompetitionID = RealisticCompetitionId,
                VRSimilatorID = Guid.Parse(vrSimulatorId),
                StartTime = now.AddMinutes(startOffsetMinutes),
                EndTime = now.AddMinutes(endOffsetMinutes),
                LimitTime = TimeSpan.FromMinutes(30)
            };
        }
    }
}