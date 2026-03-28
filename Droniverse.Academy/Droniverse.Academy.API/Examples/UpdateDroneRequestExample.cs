using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateDroneRequestExample : IMultipleExamplesProvider<UpdateDroneRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateDroneRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật drone",
            new UpdateDroneRequestDTO
            {
                DroneTypeID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                DroneNameVN = "Drone giám sát hạng nhẹ",
                DroneNameEN = "Light surveillance drone",
                Manufacturer = "Droniverse Tech",
                DescriptionVN = "Drone dùng cho huấn luyện bay và giám sát cơ bản.",
                DescriptionEN = "Drone for training and basic surveillance.",
                Height = 0.35f,
                Weight = 1.2f,
                Status = DroneStatus.AVAILABLE,
                Model3DLink = "https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2024/04/drone-light-la-gi-6.jpeg"
            }
        );
    }
}
