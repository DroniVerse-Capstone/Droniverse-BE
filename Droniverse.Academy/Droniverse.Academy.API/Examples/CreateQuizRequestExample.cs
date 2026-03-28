using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateQuizRequestExample : IMultipleExamplesProvider<CreateQuizRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateQuizRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo quiz",
            new CreateQuizRequestDTO
            {
                ModuleID = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                OrderIndex = 1,
                TitleVN = "Kiểm tra kiến thức an toàn bay",
                TitleEN = "Flight Safety Knowledge Check",
                DescriptionVN = "Bài kiểm tra đánh giá kiến thức an toàn cơ bản.",
                DescriptionEN = "Quiz to evaluate basic flight safety knowledge.",
                TimeLimit = 20,
                TotalScore = 10,
                PassScore = 7
            }
        );
    }
}
