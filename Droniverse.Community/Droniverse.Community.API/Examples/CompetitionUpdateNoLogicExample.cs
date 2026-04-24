using Droniverse.Community.Application.DTO.Request;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionUpdateNoLogicExample : IExamplesProvider<UpdateCompetitionNoLogicRequest>
    {
        private readonly IClock _clock;

        public CompetitionUpdateNoLogicExample(IClock clock)
        {
            _clock = clock;
        }

        public UpdateCompetitionNoLogicRequest GetExamples()
        {
            var now = _clock.Now;

            return new UpdateCompetitionNoLogicRequest
            {
                VisibleAt = now.TrimToMinute(),
                RegistrationStartDate = now.TrimToMinute(),
                RegistrationEndDate = now.TrimToMinute(),
                StartDate = now.TrimToMinute(),
                EndDate = now.TrimToMinute()
            };
        }
    }
}
