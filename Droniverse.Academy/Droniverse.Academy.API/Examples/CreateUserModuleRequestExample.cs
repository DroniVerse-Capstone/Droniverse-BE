using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateUserModuleRequestExample : IMultipleExamplesProvider<CreateUserModuleRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateUserModuleRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo user module",
            new CreateUserModuleRequestDTO
            {
                ModuleID = Guid.Parse("55555555-5555-5555-5555-555555555555")
            }
        );
    }
}
