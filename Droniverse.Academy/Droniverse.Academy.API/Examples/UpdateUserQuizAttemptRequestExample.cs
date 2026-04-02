using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateUserQuizAttemptRequestExample : IMultipleExamplesProvider<UpdateUserQuizAttemptRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateUserQuizAttemptRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật quiz attempt",
            new UpdateUserQuizAttemptRequestDTO
            {
                SubmitTime = DateTime.UtcNow
            }
        );
    }
}
