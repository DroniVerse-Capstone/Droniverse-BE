using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateAssignmentRequestExample : IMultipleExamplesProvider<CreateAssignmentRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateAssignmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Assignment nhập môn",
            new CreateAssignmentRequestDTO
            {
                TitleVN = "Bài tập lắp ráp drone cơ bản",
                TitleEN = "Basic Drone Assembly Assignment",
                DescriptionVN = "Học viên mô tả quy trình lắp ráp một drone cơ bản.",
                DescriptionEN = "Learners describe the process of assembling a basic drone.",
                Requirement = "Chụp ảnh các bước lắp ráp và nộp một file mô tả ngắn.",
                EstimatedTime = 60
            }
        );

        yield return SwaggerExample.Create(
            "Assignment thực hành bay",
            new CreateAssignmentRequestDTO
            {
                TitleVN = "Thiết lập waypoint bay thử nghiệm",
                TitleEN = "Set Up a Test Flight Waypoint Plan",
                DescriptionVN = "Xây dựng kịch bản waypoint đơn giản cho một chuyến bay thử nghiệm.",
                DescriptionEN = "Build a simple waypoint scenario for a test flight.",
                Requirement = "Nộp kế hoạch waypoint và giải thích lựa chọn điểm bay.",
                EstimatedTime = 90
            }
        );
    }
}

public class UpdateAssignmentRequestExample : IMultipleExamplesProvider<UpdateAssignmentRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateAssignmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Cập nhật assignment",
            new UpdateAssignmentRequestDTO
            {
                TitleVN = "Bài tập lắp ráp drone nâng cao",
                TitleEN = "Advanced Drone Assembly Assignment",
                DescriptionVN = "Bổ sung yêu cầu kiểm tra an toàn trước khi bay.",
                DescriptionEN = "Add pre-flight safety inspection requirements.",
                Requirement = "Nộp checklist an toàn và mô tả phần thay đổi.",
                EstimatedTime = 75
            }
        );
    }
}