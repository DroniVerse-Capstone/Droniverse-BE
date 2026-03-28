using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateUserQuizAttemptRequestExample : IMultipleExamplesProvider<CreateUserQuizAttemptRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateUserQuizAttemptRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo quiz attempt",
            new CreateUserQuizAttemptRequestDTO
            {
                QuizID = Guid.Parse("66666666-6666-6666-6666-666666666666")
            }
        );
    }
}
