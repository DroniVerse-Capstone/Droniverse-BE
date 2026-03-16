using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class UserCertificateResponseDTO
{
    public Guid CertificateID { get; set; }
    public Guid UserID { get; set; }
    public Guid SerialNumber { get; set; }
    public DateTime AchievedDate { get; set; }
    public UserCertificateStatus Status { get; set; }
    public CertificateResponseDTO? Certificate { get; set; }
}
