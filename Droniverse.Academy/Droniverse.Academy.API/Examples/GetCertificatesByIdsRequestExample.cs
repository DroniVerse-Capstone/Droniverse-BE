using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class GetCertificatesByIdsRequestExample : IMultipleExamplesProvider<GetCertificatesByIdsRequestDTO>
{
    public IEnumerable<SwaggerExample<GetCertificatesByIdsRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ lấy danh sách chứng chỉ theo ID",
            new GetCertificatesByIdsRequestDTO
            {
                CertificateIds =
                [
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Guid.Parse("22222222-2222-2222-2222-222222222222")
                ]
            }
        );
    }
}
