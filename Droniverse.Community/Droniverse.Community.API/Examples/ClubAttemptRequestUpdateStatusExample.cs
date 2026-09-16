using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class ClubAttemptRequestUpdateStatusExample : IMultipleExamplesProvider<ClubAttemptRequestUpdateStatusDto>
    {
        public IEnumerable<SwaggerExample<ClubAttemptRequestUpdateStatusDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Approve Request",
                "Approve a club join request - automatically creates a new Participation record",
                new ClubAttemptRequestUpdateStatusDto
                {
                    Status = ClubAttemptRequestStatus.APPROVED
                });

            yield return SwaggerExample.Create(
                "Reject Request",
                "Reject a club join request",
                new ClubAttemptRequestUpdateStatusDto
                {
                    Status = ClubAttemptRequestStatus.REJECT
                });
        }
    }
}
