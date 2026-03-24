using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateModuleRequestExample : IMultipleExamplesProvider<UpdateModuleRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateModuleRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật mô-đun",
            new UpdateModuleRequestDTO
            {
                TitleVN = "Mô-đun điều khiển drone nâng cao",
                TitleEN = "Advanced Drone Control",
                ModuleNumber = 2
            }
        );
    }
}
