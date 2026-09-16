using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
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
                ReferenceID = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                ReportType = ReportType.CourseVersion,
                ContentVN = "Lab bi loi checkpoint tai buoc 3, khong the hoan thanh bai.",
                ContentEN = "Lab checkpoint breaks at step 3 and cannot be completed."
            }
        );
    }
}
