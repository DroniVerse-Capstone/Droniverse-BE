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
                CertificateName = "Chứng chỉ hoàn thành khóa học Drone Cơ Bản",
                ImageUrl = "https://cdn.droniverse.vn/certificates/template-basic.png",
                LogoCertificate = "https://cdn.droniverse.vn/logos/droniverse-logo.png",
                Description = "Chứng nhận học viên đã hoàn thành khóa học và đạt yêu cầu đánh giá.",
                Signature = "Nguyễn Văn A",
                AuthorName = "Học viện Droniverse"
            }
        );
    }
}
