using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateReportRequestExample : IMultipleExamplesProvider<UpdateReportRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateReportRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật report",
            new UpdateReportRequestDTO
            {
                Content = "Cập nhật: lỗi xảy ra khi bay qua waypoint cuối cùng."
            }
        );
    }
}
