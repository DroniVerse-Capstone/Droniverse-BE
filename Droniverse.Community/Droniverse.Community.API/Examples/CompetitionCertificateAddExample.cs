using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class CompetitionCertificateAddExample : IMultipleExamplesProvider<CompetitionCertificateAddDto>
    {
        private static readonly Guid Cert1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid Cert2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public IEnumerable<SwaggerExample<CompetitionCertificateAddDto>> GetExamples()
        {
            // 1. Add single
            yield return SwaggerExample.Create(
                "Add Single Certificate",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid> { Cert1 }
                }
            );

            // 2. Add multiple
            yield return SwaggerExample.Create(
                "Add Multiple Certificates",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid> { Cert1, Cert2 }
                }
            );

            // 3. Duplicate (simulate already exists → skip)
            yield return SwaggerExample.Create(
                "Add Certificates - Duplicate (Skip Existing)",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid> { Cert1, Cert1 }
                }
            );

            // 4. Empty list (error case)
            yield return SwaggerExample.Create(
                "Error - Empty List",
                new CompetitionCertificateAddDto
                {
                    CertificateIDs = new List<Guid>()
                }
            );
        }
    }
}