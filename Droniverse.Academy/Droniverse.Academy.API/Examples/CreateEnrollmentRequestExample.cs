using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateEnrollmentRequestExample : IMultipleExamplesProvider<CreateEnrollmentRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateEnrollmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo enrollment",
            new CreateEnrollmentRequestDTO
            {
                CourseVersionID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ClubID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ExpireDate = DateTime.UtcNow.AddMonths(6)
            }
        );
    }
}
