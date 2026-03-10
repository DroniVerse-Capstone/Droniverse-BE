using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionCertificateAddExample : IMultipleExamplesProvider<CompetitionCertificateAddDto>
    {
        public IEnumerable<SwaggerExample<CompetitionCertificateAddDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Add Single Certificate",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>
                    {
                        Guid.Parse("11111111-1111-1111-1111-111111111111")
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Add Multiple Certificates - Standard Package",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>
                    {
                        Guid.Parse("11111111-1111-1111-1111-111111111111"), // Winner Top 1
                        Guid.Parse("22222222-2222-2222-2222-222222222222"), // Top 3
                        Guid.Parse("33333333-3333-3333-3333-333333333333")  // Participation
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Add Multiple Certificates - Full Package",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>
                    {
                        Guid.Parse("11111111-1111-1111-1111-111111111111"), // Winner Top 1
                        Guid.Parse("22222222-2222-2222-2222-222222222222"), // Top 3
                        Guid.Parse("33333333-3333-3333-3333-333333333333"), // Participation
                        Guid.Parse("44444444-4444-4444-4444-444444444444"), // Excellence Award
                        Guid.Parse("55555555-5555-5555-5555-555555555555")  // Best Performance
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Add Certificates - Some Already Exist (Skip)",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>
                    {
                        Guid.Parse("11111111-1111-1111-1111-111111111111"), // Đã tồn tại - skip
                        Guid.Parse("66666666-6666-6666-6666-666666666666"), // Mới - add
                        Guid.Parse("77777777-7777-7777-7777-777777777777")  // Mới - add
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Error - Invalid Certificate ID",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>
                    {
                        Guid.Parse("99999999-9999-9999-9999-999999999999"), // Không tồn tại trong Academy
                        Guid.Parse("88888888-8888-8888-8888-888888888888")  // Không tồn tại trong Academy
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Error - Empty List",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>() // ❌ Empty list
                }
            );
        }
    }
}
