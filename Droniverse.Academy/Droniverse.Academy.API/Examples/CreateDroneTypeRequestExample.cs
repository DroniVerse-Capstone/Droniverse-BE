using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateDroneTypeRequestExample : IMultipleExamplesProvider<CreateDroneTypeRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateDroneTypeRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo loại drone",
            new CreateDroneTypeRequestDTO
            {
                TypeNameVN = "Drone giám sát",
                TypeNameEN = "Surveillance drone",
                DescriptionVN = "Loại drone dùng để giám sát khu vực và thu thập hình ảnh.",
                DescriptionEN = "Drone type used for area surveillance and image collection."
            }
        );
    }
}
