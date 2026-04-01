using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateUserLessonRequestExample : IMultipleExamplesProvider<CreateUserLessonRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateUserLessonRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo user lesson",
            new CreateUserLessonRequestDTO
            {
                LessonID = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Status = UserLessonStatus.INCOMPLETED,
                Progress = 25f
            }
        );
    }
}
