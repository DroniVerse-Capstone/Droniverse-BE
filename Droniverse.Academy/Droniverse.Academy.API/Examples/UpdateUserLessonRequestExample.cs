using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateUserLessonRequestExample : IMultipleExamplesProvider<UpdateUserLessonRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateUserLessonRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật user lesson",
            new UpdateUserLessonRequestDTO
            {
                Status = UserLessonStatus.COMPLETED,
                Progress = 100f,
                LastAccessDate = DateTime.UtcNow
            }
        );
    }
}
