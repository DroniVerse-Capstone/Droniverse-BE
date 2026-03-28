using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateCourseVersionRequestExample : IMultipleExamplesProvider<UpdateCourseVersionRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateCourseVersionRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật phiên bản khóa học",
            new UpdateCourseVersionRequestDTO
            {
                TitleVN = "Điều khiển drone nâng cao",
                TitleEN = "Advanced Drone Control",
                DescriptionVN = "Nâng cấp nội dung với tình huống bay thực tế.",
                DescriptionEN = "Upgraded with real-world flying scenarios.",
                ContextVN = "Tập trung vào kỹ năng xử lý sự cố trong khi bay.",
                ContextEN = "Focus on in-flight incident handling skills.",
                ImageUrl = "https://doanhnhanplus.vn/wp-content/uploads/2020/02/dnp-nhung-hinh-anh-kinh-ngac-bat-duoc-tu-drone-1-1140x712.jpg",
                Level = CourseLevel.MEDIUM,
                EstimatedDuration = 180,
                ChangeLog = "Bổ sung tình huống bay thực tế và tăng thời lượng thực hành."
            }
        );
    }
}
