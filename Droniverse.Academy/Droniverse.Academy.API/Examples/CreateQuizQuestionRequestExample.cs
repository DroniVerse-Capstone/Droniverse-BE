using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateQuizQuestionRequestExample : IMultipleExamplesProvider<CreateQuizQuestionRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateQuizQuestionRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo câu hỏi quiz",
            new CreateQuizQuestionRequestDTO
            {
                ContentVN = "Độ cao bay an toàn tối thiểu khi cất cánh trong khu vực trống là bao nhiêu?",
                ContentEN = "What is the minimum safe takeoff altitude in an open area?",
                AnswerA = "1 mét",
                AnswerB = "3 mét",
                AnswerC = "5 mét",
                AnswerD = "10 mét",
                CorrectAnswer = "C",
                Score = 1
            }
        );
    }
}
