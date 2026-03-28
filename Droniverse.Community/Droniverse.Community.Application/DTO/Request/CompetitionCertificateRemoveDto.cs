using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionCertificateRemoveDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 certificate")]
        public List<Guid> CertificateIDs { get; set; }
    }
}
