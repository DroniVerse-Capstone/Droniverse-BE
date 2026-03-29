using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AddRequiredDronesRequestExample : IMultipleExamplesProvider<AddRequiredDronesRequestDTO>
{
    public IEnumerable<SwaggerExample<AddRequiredDronesRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ gán nhiều drone yêu cầu",
            new AddRequiredDronesRequestDTO
            {
                DroneIDs =
                [
                    Guid.Parse("30876cff-7818-44ab-8fee-bc2b99c36e7c"),
                    Guid.Parse("5f167938-c022-4529-ab73-5f91616b150e"),
                    Guid.Parse("3aadf987-4f4f-4136-be4b-5fd906a0a9e4")
                ]
            }
        );
    }
}
