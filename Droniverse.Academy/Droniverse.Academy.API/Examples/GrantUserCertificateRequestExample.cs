using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class GrantUserCertificateRequestExample : IMultipleExamplesProvider<GrantUserCertificateRequestDTO>
{
    public IEnumerable<SwaggerExample<GrantUserCertificateRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cấp chứng chỉ cho người dùng",
            new GrantUserCertificateRequestDTO
            {
                CertificateID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserID = Guid.Parse("22222222-2222-2222-2222-222222222222")
            }
        );
    }
}
