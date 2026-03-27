using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionUpdateStatusExample : IMultipleExamplesProvider<CompetitionUpdateStatusDto>
    {
        public IEnumerable<SwaggerExample<CompetitionUpdateStatusDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "01. Publish Competition",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.PUBLISHED
                }
            );

            yield return SwaggerExample.Create(
                "02. Open Registration",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.REGISTRATION_OPEN
                }
            );

            yield return SwaggerExample.Create(
                "03. Close Registration",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.REGISTRATION_CLOSED
                }
            );

            yield return SwaggerExample.Create(
                "04. Start Competition",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.ONGOING
                }
            );

            yield return SwaggerExample.Create(
                "05. Finish Competition",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.FINISHED
                }
            );

            yield return SwaggerExample.Create(
                "06. Publish Result",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.RESULT_PUBLISHED
                }
            );

            yield return SwaggerExample.Create(
                "07. Cancel Competition",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.CANCELLED
                }
            );

            yield return SwaggerExample.Create(
                "08. Mark Competition Invalid",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.INVALID,
                    InvalidReason = CompetitionInvalidReason.Unknown
                }
            );
        }
    }
}
