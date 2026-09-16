using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateUserLabRequestExample : IMultipleExamplesProvider<UpdateUserLabRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateUserLabRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật user lab",
            new UpdateUserLabRequestDTO
            {
                Solution = @"{""route"": [""A"", ""D"", ""C""]}",
                IsCompleted = true,
                Time = 98.2f,
                NumberOfStep = 15,
                Length = 520.3f,
                FeedbackVN = "Đã hoàn thành tốt.",
                FeedbackEN = "Completed well.",
                Point = 9.2m
            }
        );
    }
}
