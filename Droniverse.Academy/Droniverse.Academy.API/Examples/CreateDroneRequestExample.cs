using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateDroneRequestExample : IMultipleExamplesProvider<CreateDroneRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateDroneRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo drone",
            new CreateDroneRequestDTO
            {
                DroneNameVN = "Drone khảo sát địa hình",
                DroneNameEN = "Terrain survey drone",
                Manufacturer = "Droniverse Tech",
                DescriptionVN = "Drone phục vụ khảo sát và huấn luyện bay cơ bản.",
                DescriptionEN = "Drone for surveying and basic flight training.",
                Height = 0.42f,
                Weight = 1.35f,
                Status = DroneStatus.DRAFT,
                Model3DLink = "https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2024/04/drone-light-la-gi-6.jpeg"
            }
        );
    }
}
