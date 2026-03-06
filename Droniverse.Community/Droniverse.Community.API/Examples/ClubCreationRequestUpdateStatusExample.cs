using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class ClubCreationRequestUpdateStatusExample : IMultipleExamplesProvider<ClubCreationRequestUpdateStatusDto>
    {
        public IEnumerable<SwaggerExample<ClubCreationRequestUpdateStatusDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Approve Request",
                "Approve a club creation request - automatically creates a new active club",
                new ClubCreationRequestUpdateStatusDto
                {
                    Status = ClubCreationRequestStatus.APPROVED,
                    RejectReason = null
                });

            yield return SwaggerExample.Create(
                "Reject Request",
                "Reject a club creation request with a reason",
                new ClubCreationRequestUpdateStatusDto
                {
                    Status = ClubCreationRequestStatus.REJECTED,
                    RejectReason = "Tên câu lạc bộ không phù hợp với tiêu chí. Vui lòng cập nhật thông tin và gửi lại yêu cầu."
                });

            yield return SwaggerExample.Create(
                "Cancel Request",
                "Cancel a pending club creation request",
                new ClubCreationRequestUpdateStatusDto
                {
                    Status = ClubCreationRequestStatus.CANCEL,
                    RejectReason = null
                });
        }
    }
}
