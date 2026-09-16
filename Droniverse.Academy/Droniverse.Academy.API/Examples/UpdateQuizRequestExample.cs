using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateQuizRequestExample : IMultipleExamplesProvider<UpdateQuizRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateQuizRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật quiz",
            new UpdateQuizRequestDTO
            {
                TitleVN = "Kiểm tra kiến thức điều hướng",
                TitleEN = "Navigation Knowledge Check",
                DescriptionVN = "Bổ sung câu hỏi về điều hướng trong điều kiện gió.",
                DescriptionEN = "Added navigation questions under windy conditions.",
                TimeLimit = 25,
                TotalScore = 10,
                PassScore = 8
            }
        );
    }
}
