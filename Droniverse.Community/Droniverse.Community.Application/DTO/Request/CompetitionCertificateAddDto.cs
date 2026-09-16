using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionCertificateAddDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 chứng chỉ")]
        public List<Guid> CertificateIDs { get; set; }
    }
}
