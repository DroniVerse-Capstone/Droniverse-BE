using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AssignCategoriesRequestExample : IMultipleExamplesProvider<AssignCategoriesRequestDTO>
{
    public IEnumerable<SwaggerExample<AssignCategoriesRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ gán nhiều category cho phiên bản khóa học",
            new AssignCategoriesRequestDTO
            {
                CategoryIDs =
                [
                    Guid.Parse("0b27da26-062c-4ecd-8b6f-3f895d21ae4f"),
                    Guid.Parse("44ca2075-50fd-4b8f-a60c-4db6ad7cc708")
                ]
            }
        );
    }
}
