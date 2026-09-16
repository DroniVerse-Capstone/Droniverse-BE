using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class RespondReportRequestExample : IMultipleExamplesProvider<RespondReportRequestDTO>
{
    public IEnumerable<SwaggerExample<RespondReportRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ phản hồi report",
            new RespondReportRequestDTO
            {
                ResponseVN = "Đã ghi nhận lỗi và sẽ cập nhật trong bản vá sắp tới.",
                ResponseEN = "Issue acknowledged. A fix will be included in the next patch."
            }
        );
    }
}
