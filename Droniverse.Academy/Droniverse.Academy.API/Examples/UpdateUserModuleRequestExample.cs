using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateUserModuleRequestExample : IMultipleExamplesProvider<UpdateUserModuleRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateUserModuleRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật user module",
            new UpdateUserModuleRequestDTO
            {
                Progress = 100f,
                IsCompleted = true
            }
        );
    }
}
