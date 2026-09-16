using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class UpdateCertificateRequestExample : IMultipleExamplesProvider<UpdateCertificateRequestDTO>
{
    public IEnumerable<SwaggerExample<UpdateCertificateRequestDTO>> GetExamples()
    {
        yield return SwaggerExample.Create(
            "Ví dụ cập nhật chứng chỉ",
            new UpdateCertificateRequestDTO
            {
                CertificateNameVN = "Chứng chỉ nâng cao Drone Navigation",
                CertificateNameEN = "Advanced Drone Navigation Certificate",
                ImageUrl = "https://cdn.droniverse.vn/certificates/template-advanced.png",
                LogoCertificate = "https://cdn.droniverse.vn/logos/droniverse-logo-new.png",
                Description = "Mẫu chứng chỉ cập nhật cho chương trình nâng cao.",
                Signature = "Trần Thị B",
                AuthorName = "Trung tâm đào tạo Droniverse"
            }
        );
    }
}
