using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionCertificateAddDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Ph?i có ít nh?t 1 certificate")]
        public List<Guid> CertificateIDs { get; set; }
    }
}
