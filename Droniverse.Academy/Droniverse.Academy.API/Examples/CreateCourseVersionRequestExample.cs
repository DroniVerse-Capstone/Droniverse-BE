using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateCourseVersionRequestExample : IMultipleExamplesProvider<CreateCourseVersionRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateCourseVersionRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo phiên bản khóa học",
            new CreateCourseVersionRequestDTO
            {
                TitleVN = "Điều khiển drone cơ bản",
                TitleEN = "Basic Drone Control",
                DescriptionVN = "Phiên bản dành cho người mới bắt đầu.",
                DescriptionEN = "Version for beginner learners.",
                ContextVN = "Học cách cất cánh, hạ cánh và điều hướng an toàn.",
                ContextEN = "Learn takeoff, landing and safe navigation.",
                ImageUrl = "https://photo2.tinhte.vn/data/attachment-files/2023/07/6507648_dji-air-3-ra-mat-tinhte-47.jpg",
                EstimatedDuration = 120,
                ChangeLog = "Khởi tạo phiên bản đầu tiên với nội dung nền tảng cho người mới."
            }
        );
    }
}
