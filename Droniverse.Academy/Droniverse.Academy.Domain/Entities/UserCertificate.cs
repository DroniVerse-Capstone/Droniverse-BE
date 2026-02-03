
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Entities;
public class UserCertificate
{
    public Certificate Certificate { get; set; }
    public Guid CertificateID { get; set; }
    public Guid UserID { get; set; } // reference to UserID
    public Guid SerialNumber { get; set; } //char(36)
    public DateTime AchievedDate { get; set; }
    public UserCertificateStatus Status { get; set; } //
}
