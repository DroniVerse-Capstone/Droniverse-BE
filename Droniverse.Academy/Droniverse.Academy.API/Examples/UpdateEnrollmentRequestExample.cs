using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateEnrollmentRequestExample : IMultipleExamplesProvider<UpdateEnrollmentRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateEnrollmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật enrollment (đầy đủ)",
            new UpdateEnrollmentRequestDTO
            {
                Progress = 45.5f,
                LastAccessDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddMonths(6),
            }
        );

        yield return SwaggerExample.Create(
            "Ví dụ cập nhật enrollment (PATCH một phần)",
            new UpdateEnrollmentRequestDTO
            {
                Progress = 60f
            }
        );
    }
}
