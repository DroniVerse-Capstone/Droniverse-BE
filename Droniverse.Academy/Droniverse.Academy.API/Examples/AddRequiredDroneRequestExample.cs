using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AddRequiredDroneRequestExample : IMultipleExamplesProvider<AddRequiredDroneRequestDTO>
{
    public IEnumerable<SwaggerExample<AddRequiredDroneRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ gán drone yêu cầu",
            new AddRequiredDroneRequestDTO
            {
                DroneID = Guid.Parse("11111111-1111-1111-1111-111111111111")
            }
        );
    }
}
