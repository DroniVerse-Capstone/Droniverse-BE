using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class UserRoundSubmitExample : IExamplesProvider<UserRoundSubmitDto>
    {
        public UserRoundSubmitDto GetExamples()
        {
            return new UserRoundSubmitDto
            {
                Solution = "{\"commands\": [\"FORWARD\", \"TURN_LEFT\", \"FORWARD\", \"LAND\"]}"
            };
        }
    }
}
