using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class FeedbackCreateRequestExample : IMultipleExamplesProvider<FeedbackCreateDTO>
{
    public IEnumerable<SwaggerExample<FeedbackCreateDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo phản hồi tốt",
            new FeedbackCreateDTO
            {
                Rating = 5,
                Content = "Khóa học rất chi tiết và dễ hiểu, giảng viên hỗ trợ tốt.",
                CourseVersionID = Guid.Parse("08fd3eac-8cdc-4c08-afd1-021bbd4a34e5")
            }
        );

        yield return SwaggerExample.Create(
           "Ví dụ tạo phản xấu",
           new FeedbackCreateDTO
           {
               Rating = 3,
               Content = "Khóa học không phù hợp với, nội dung khó hiểu.",
               CourseVersionID = Guid.Parse("08fd3eac-8cdc-4c08-afd1-021bbd4a34e5")
           }
       );
    }
}
