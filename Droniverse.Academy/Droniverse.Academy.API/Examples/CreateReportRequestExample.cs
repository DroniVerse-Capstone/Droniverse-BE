using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateReportRequestExample : IMultipleExamplesProvider<CreateReportRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateReportRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo report",
            new CreateReportRequestDTO
            {
                LabID = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Content = "Lab bị lỗi checkpoint tại bước 3, không thể hoàn thành bài."
            }
        );
    }
}
