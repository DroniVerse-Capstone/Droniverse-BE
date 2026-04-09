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
                "02. Publish Result",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.RESULT_PUBLISHED
                }
            );

            yield return SwaggerExample.Create(
                "03. Cancel Competition",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.CANCELLED
                }
            );

            yield return SwaggerExample.Create(
                "04. Mark Competition Invalid",
                new CompetitionUpdateStatusDto
                {
                    Status = CompetitionStatus.INVALID,
                    InvalidReason = CompetitionInvalidReason.Unknown
                }
            );
        }
    }
}
