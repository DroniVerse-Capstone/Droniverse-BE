namespace Droniverse.Community.Domain.Entities;
public class CompetitionCertificate
{
    public Competition Competition { get; set; }
    public Guid CompetitionID { get; set; } //char(36)
    //public Certificate Certificate { get; set; }
    public Guid CertificateID { get; set; } //char(36)
}

