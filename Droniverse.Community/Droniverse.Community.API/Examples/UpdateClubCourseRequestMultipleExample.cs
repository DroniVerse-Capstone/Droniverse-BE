using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples;

public class UpdateClubCourseRequestMultipleExample : IMultipleExamplesProvider<UpdateClubCourseRequest>
{
    public IEnumerable<SwaggerExample<UpdateClubCourseRequest>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Cập nhật sang NONPROFIT",
            "Ko lợi nhuận",
            new UpdateClubCourseRequest
            {
                ProfitType = ClubCourseProfit.NONPROFIT
            });

        yield return SwaggerExample.Create(
            "Cập nhật sang PROFIT",
            "Có lợi nhuận",
            new UpdateClubCourseRequest
            {
                ProfitType = ClubCourseProfit.PROFIT
            });
    }
}
