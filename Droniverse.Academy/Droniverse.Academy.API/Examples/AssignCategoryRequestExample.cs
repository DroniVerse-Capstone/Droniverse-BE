using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AssignCategoryRequestExample : IMultipleExamplesProvider<AssignCategoryRequestDTO>
{
    public IEnumerable<SwaggerExample<AssignCategoryRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ gán danh mục cho phiên bản khóa học",
            new AssignCategoryRequestDTO
            {
                CategoryID = Guid.Parse("44ca2075-50fd-4b8f-a60c-4db6ad7cc708")
            }
        );
    }
}
