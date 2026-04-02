using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateUserQuizQuestionAttemptRequestExample : IMultipleExamplesProvider<CreateUserQuizQuestionAttemptRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateUserQuizQuestionAttemptRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo quiz question attempt",
            new CreateUserQuizQuestionAttemptRequestDTO
            {
                AttemptID = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                QuestionID = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                SelectedAnswer = "B"
            }
        );
    }
}
