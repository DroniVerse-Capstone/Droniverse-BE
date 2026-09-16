namespace Droniverse.Community.Domain.Entities;
public class CompetitionCertificate
{
    public Guid CompetitionID { get; set; } //char(36)
    public Guid CertificateID { get; set; } //char(36)
    public Competition Competition { get; set; }
}

