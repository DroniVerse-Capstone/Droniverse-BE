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
            };
        }
    }
}
