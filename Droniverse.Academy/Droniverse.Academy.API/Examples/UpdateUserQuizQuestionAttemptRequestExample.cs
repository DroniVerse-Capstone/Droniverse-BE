using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateUserQuizQuestionAttemptRequestExample : IMultipleExamplesProvider<UpdateUserQuizQuestionAttemptRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateUserQuizQuestionAttemptRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật quiz question attempt",
            new UpdateUserQuizQuestionAttemptRequestDTO
            {
                SelectedAnswer = "C"
            }
        );
    }
}
