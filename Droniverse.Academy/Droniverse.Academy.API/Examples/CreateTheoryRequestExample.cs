using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateTheoryRequestExample : IMultipleExamplesProvider<CreateTheoryRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateTheoryRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo bài lý thuyết",
            new CreateTheoryRequestDTO
            {
                ModuleID = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                OrderIndex = 1,
                TitleVN = "Quy tắc an toàn bay cơ bản",
                TitleEN = "Basic Flight Safety Rules",
                ContentVN = "Nội dung lý thuyết về các quy tắc an toàn trước khi bay.",
                ContentEN = "Theory content about pre-flight safety rules.",
                EstimatedTime = 15
            }
        );
    }
}
