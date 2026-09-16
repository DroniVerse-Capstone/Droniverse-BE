using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateLessonRequestExample : IMultipleExamplesProvider<UpdateLessonRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateLessonRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật bài học",
            new UpdateLessonRequestDTO
            {
                OrderIndex = 2,
                Type = LessonType.THEORY,
                ReferenceID = Guid.Parse("33333333-3333-3333-3333-333333333333")
            }
        );
    }
}
