using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateLabRequestExample : IMultipleExamplesProvider<UpdateLabRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateLabRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Update lab learning",
            new UpdateLabRequestDTO
            {
                Type = LabType.LEARNING,
                Level = LabLevel.MEDIUM,
                Status = LabStatus.ACTIVE,
                EstimatedTime = 25,
                NameVN = "Lab điều khiển drone theo waypoint nâng cao",
                NameEN = "Advanced Waypoint Navigation Lab",
                DescriptionVN = "Cập nhật nội dung thực hành điều hướng waypoint ở mức nâng cao.",
                DescriptionEN = "Updated advanced waypoint navigation practice content."
            }
        );

        yield return SwaggerExample.Create(
            "Update lab competition",
            new UpdateLabRequestDTO
            {
                Type = LabType.COMPETITION,
                Level = LabLevel.HARD,
                Status = LabStatus.LOCKED,
                EstimatedTime = 45,
                NameVN = "Lab thi đấu tốc độ phản xạ",
                NameEN = "Reaction Speed Competition Lab",
                DescriptionVN = "Cập nhật bài lab thi đấu yêu cầu phản xạ nhanh trong giới hạn thời gian.",
                DescriptionEN = "Updated competition lab requiring quick reactions under time constraints."
            }
        );
    }
}
