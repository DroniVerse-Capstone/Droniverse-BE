using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class AdminUpdateEnrollmentRequestExample : IMultipleExamplesProvider<AdminUpdateEnrollmentRequestDTO>
{
    public IEnumerable<SwaggerExample<AdminUpdateEnrollmentRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật enrollment bởi admin",
            new AdminUpdateEnrollmentRequestDTO
            {
                Progress = 80f,
                LastAccessDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddMonths(3),
                Status = EnrollStatus.ACTIVE
            }
        );
    }
}
