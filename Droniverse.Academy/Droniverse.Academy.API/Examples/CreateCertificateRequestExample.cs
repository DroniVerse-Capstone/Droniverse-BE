using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateCertificateRequestExample : IMultipleExamplesProvider<CreateCertificateRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateCertificateRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ tạo chứng chỉ",
            new CreateCertificateRequestDTO
            {
                CertificateNameVN = "Chứng chỉ hoàn thành khóa học Drone Cơ Bản",
                CertificateNameEN = "Certificate of Completion - Basic Drone Course"
            }
        );
    }
}
