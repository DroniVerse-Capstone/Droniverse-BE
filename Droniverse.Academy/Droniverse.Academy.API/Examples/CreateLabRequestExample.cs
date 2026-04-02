using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateLabRequestExample : IMultipleExamplesProvider<CreateLabRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateLabRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Learning - Easy - Draft",
            new CreateLabRequestDTO
            {
                Type = LabType.LEARNING,
                Level = LabLevel.EASY,
                Status = LabStatus.DRAFT,
                NameVN = "Lab cất/hạ cánh cơ bản",
                NameEN = "Basic Takeoff and Landing Lab",
                DescriptionVN = "Thực hành thao tác cất cánh và hạ cánh an toàn.",
                DescriptionEN = "Practice safe takeoff and landing maneuvers."
            }
        );

        yield return SwaggerExample.Create(
            "Learning - Easy - Active",
            new CreateLabRequestDTO
            {
                Type = LabType.LEARNING,
                Level = LabLevel.EASY,
                Status = LabStatus.ACTIVE,
                NameVN = "Lab bay theo hình vuông",
                NameEN = "Square Pattern Flight Lab",
                DescriptionVN = "Điều khiển drone bay theo quỹ đạo hình vuông.",
                DescriptionEN = "Control the drone to fly in a square pattern."
            }
        );

        yield return SwaggerExample.Create(
            "Learning - Medium - Active",
            new CreateLabRequestDTO
            {
                Type = LabType.LEARNING,
                Level = LabLevel.MEDIUM,
                Status = LabStatus.ACTIVE,
                NameVN = "Lab bay theo waypoint",
                NameEN = "Waypoint Navigation Lab",
                DescriptionVN = "Thực hành tạo và bay theo danh sách waypoint.",
                DescriptionEN = "Practice creating and following waypoint routes."
            }
        );

        yield return SwaggerExample.Create(
            "Learning - Hard - Inactive",
            new CreateLabRequestDTO
            {
                Type = LabType.LEARNING,
                Level = LabLevel.HARD,
                Status = LabStatus.INACTIVE,
                NameVN = "Lab tránh vật cản nâng cao",
                NameEN = "Advanced Obstacle Avoidance Lab",
                DescriptionVN = "Huấn luyện thuật toán tránh vật cản trong môi trường phức tạp.",
                DescriptionEN = "Train obstacle avoidance in a complex environment."
            }
        );

        yield return SwaggerExample.Create(
            "Competition - Medium - Active",
            new CreateLabRequestDTO
            {
                Type = LabType.COMPETITION,
                Level = LabLevel.MEDIUM,
                Status = LabStatus.ACTIVE,
                NameVN = "Lab thi đấu vượt chướng ngại",
                NameEN = "Obstacle Course Competition Lab",
                DescriptionVN = "Bài lab mô phỏng thi đấu vượt chướng ngại vật.",
                DescriptionEN = "Competition-style obstacle course challenge lab."
            }
        );

        yield return SwaggerExample.Create(
            "Competition - Hard - Locked",
            new CreateLabRequestDTO
            {
                Type = LabType.COMPETITION,
                Level = LabLevel.HARD,
                Status = LabStatus.LOCKED,
                NameVN = "Lab tốc độ phản xạ",
                NameEN = "Reaction Speed Challenge Lab",
                DescriptionVN = "Lab thi đấu yêu cầu phản xạ nhanh theo tín hiệu ngẫu nhiên.",
                DescriptionEN = "Competition lab requiring quick reactions to random signals."
            }
        );

        yield return SwaggerExample.Create(
            "Competition - Hard - Deleted",
            new CreateLabRequestDTO
            {
                Type = LabType.COMPETITION,
                Level = LabLevel.HARD,
                Status = LabStatus.DELETED,
                NameVN = "Lab đua vòng kín (đã ngừng)",
                NameEN = "Closed Circuit Racing Lab (Deprecated)",
                DescriptionVN = "Mẫu dữ liệu lab đã ngừng sử dụng để kiểm thử luồng quản trị.",
                DescriptionEN = "Deprecated lab sample for admin/testing workflow."
            }
        );
    }
}
