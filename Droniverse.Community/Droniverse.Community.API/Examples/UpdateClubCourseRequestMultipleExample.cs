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
            "Điều chỉnh tổng số lượng và loại lợi nhuận về phi lợi nhuận",
            new UpdateClubCourseRequest
            {
                TotalQuantity = 80,
                ProfitType = ClubCourseProfit.NONPROFIT
            });

        yield return SwaggerExample.Create(
            "Cập nhật sang PROFIT",
            "Điều chỉnh tổng số lượng và loại lợi nhuận về có lợi nhuận",
            new UpdateClubCourseRequest
            {
                TotalQuantity = 120,
                ProfitType = ClubCourseProfit.PROFIT
            });
    }
}
