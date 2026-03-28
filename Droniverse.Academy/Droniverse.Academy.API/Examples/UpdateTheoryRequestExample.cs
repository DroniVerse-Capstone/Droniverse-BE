using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateTheoryRequestExample : IMultipleExamplesProvider<UpdateTheoryRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateTheoryRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật bài lý thuyết",
            new UpdateTheoryRequestDTO
            {
                TitleVN = "Xử lý drone khi gió mạnh",
                TitleEN = "Handling Drone in Strong Wind",
                ContentVN = "Nội dung đã cập nhật: bổ sung quy trình xử lý khi drone gặp gió mạnh.",
                ContentEN = "Updated content: added handling process for strong wind conditions.",
                EstimatedTime = 20
            }
        );
    }
}
