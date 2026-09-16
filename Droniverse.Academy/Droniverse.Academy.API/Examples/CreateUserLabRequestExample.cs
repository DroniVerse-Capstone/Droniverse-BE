using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateUserLabRequestExample : IMultipleExamplesProvider<CreateUserLabRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateUserLabRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo user lab",
            new CreateUserLabRequestDTO
            {
                LabID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Solution = @"{""route"": [""A"", ""B"", ""C""]}",
                IsCompleted = false,
                Time = 120.5f,
                NumberOfStep = 18,
                Length = 560.2f,
                FeedbackVN = "Cần tối ưu đường bay.",
                FeedbackEN = "Need to optimize the flight path.",
                Point = 8.5m
            }
        );
    }
}
