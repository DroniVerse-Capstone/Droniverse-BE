using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples;

public class AddClubCourseRequestMultipleExample : IMultipleExamplesProvider<AddClubCourseRequest>
{
    public IEnumerable<SwaggerExample<AddClubCourseRequest>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Thêm khóa học NONPROFIT",
            "Khóa học phi lợi nhuận trong câu lạc bộ",
            new AddClubCourseRequest
            {
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TotalQuantity = 50,
                ProfitType = ClubCourseProfit.NONPROFIT
            });

        yield return SwaggerExample.Create(
            "Thêm khóa học PROFIT",
            "Khóa học có lợi nhuận trong câu lạc bộ",
            new AddClubCourseRequest
            {
                CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                TotalQuantity = 100,
                ProfitType = ClubCourseProfit.PROFIT
            });
    }
}
