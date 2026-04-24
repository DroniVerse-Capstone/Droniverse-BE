using Droniverse.Community.Application.DTO.Request;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class RoundUpdateNoLogicExample : IExamplesProvider<UpdateRoundNoLogicRequest>
    {
        private readonly IClock _clock;

        public RoundUpdateNoLogicExample(IClock clock)
        {
            _clock = clock;
        }

        public UpdateRoundNoLogicRequest GetExamples()
        {
            var now = _clock.Now;

            return new UpdateRoundNoLogicRequest
            {
                StartTime = now.TrimToMinute(),
                EndTime = now.TrimToMinute(),
                TimeLimit = now.TrimToMinute().TimeOfDay
            };
        }
    }
}
