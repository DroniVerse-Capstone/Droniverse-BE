using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionCertificateAddExample : IMultipleExamplesProvider<CompetitionCertificateAddDto>
    {
        public IEnumerable<SwaggerExample<CompetitionCertificateAddDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Certificate Winner - Top 1",
                new CompetitionCertificateAddDto
                {
                    CertificateID = Guid.Parse("11111111-1111-1111-1111-111111111111")
                }
            );

            yield return SwaggerExample.Create(
                "Certificate Top 3",
                new CompetitionCertificateAddDto
                {
                    CertificateID = Guid.Parse("22222222-2222-2222-2222-222222222222")
                }
            );

            yield return SwaggerExample.Create(
                "Certificate Participation",
                new CompetitionCertificateAddDto
                {
                    CertificateID = Guid.Parse("33333333-3333-3333-3333-333333333333")
                }
            );

            yield return SwaggerExample.Create(
                "Error - Duplicate Certificate",
                new CompetitionCertificateAddDto
                {
                    CertificateID = Guid.Parse("11111111-1111-1111-1111-111111111111") // Giả sử đã add rồi
                }
            );
        }
    }
}
