using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class SubmitQuizRequestExample : IMultipleExamplesProvider<SubmitQuizRequestDTO>
{
    public IEnumerable<SwaggerExample<SubmitQuizRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Submit quiz với SelectedOptionKey",
            new SubmitQuizRequestDTO
            {
                Answers =
                [
                    new SubmitQuizAnswerRequestDTO
                    {
                        QuestionID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        SelectedOptionKey = "B"
                    },
                    new SubmitQuizAnswerRequestDTO
                    {
                        QuestionID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        SelectedOptionKey = "A"
                    }
                ]
            }
        );
    }
}
