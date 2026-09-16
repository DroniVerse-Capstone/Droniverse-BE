using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class FeedbackUpdateRequestExample : IMultipleExamplesProvider<FeedbackUpdateDTO>
{
    public IEnumerable<SwaggerExample<FeedbackUpdateDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật phản hồi",
            new FeedbackUpdateDTO
            {
                Rating = 4,
                Content = "Nội dung tốt, mong muốn có thêm nhiều bài thực hành hơn."
            }
        );
    }
}
