using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Domain.Entities;
public class UserCertificate
{
    public Certificate Certificate { get; set; }
    public Guid CertificateID { get; set; }
    public Guid UserID { get; set; } // reference to UserID
    public required string CertificateUrl { get; set; } //char(36)
    public DateTime AchievedDate { get; set; }
    public UserCertificateStatus Status { get; private set; }

    public void Achieve(DateTime achievedDate)
    {
        if (Status == UserCertificateStatus.ACHIEVED)
            throw new DomainException("Certificate is already achieved.");

        Status = UserCertificateStatus.ACHIEVED;
        AchievedDate = achievedDate;
    }

    public void Revoke()
    {
        if (Status != UserCertificateStatus.ACHIEVED)
            throw new DomainException($"Cannot revoke certificate from status {Status}");

        Status = UserCertificateStatus.REVOKED;
    }

    public void TransitionTo(UserCertificateStatus targetStatus, DateTime? achievedDate = null)
    {
        if (Status == targetStatus)
            return;

        switch (targetStatus)
        {
            case UserCertificateStatus.ACHIEVED:
                Achieve(achievedDate ?? DateTime.UtcNow);
                break;
            case UserCertificateStatus.REVOKED:
                Revoke();
                break;
            default:
                throw new DomainException($"Unsupported user certificate status {targetStatus}");
        }
    }
}
