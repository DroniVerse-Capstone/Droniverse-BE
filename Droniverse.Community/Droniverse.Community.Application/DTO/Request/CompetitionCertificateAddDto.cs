using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public class CompetitionCertificateAddDto
    {
        [Required]
        public Guid CertificateID { get; set; }
    }
}
