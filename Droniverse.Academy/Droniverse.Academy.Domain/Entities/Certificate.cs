namespace Droniverse.Academy.Domain.Entities;
public class Certificate
{
    public Guid CertificateID { get; set; }

    public Guid UpdateBy { get; set; } // reference to UserID
    public Guid CourseVersionID { get; set; }
    public ICollection<UserCertificate> UserCertificates { get; set; }
    public Course Course { get; set; }
    public Guid CourseID { get; set; }
    //public ICollection<CompetitionCertificate> CompetitionCertificates { get; set; }

    public string CertificateName { get; set; } //varchar(255)
    public string ImageUrl { get; set; } //varchar(255)
    public string LogoCertificate { get; set; }//text
    public string Description { get; set; } //text
    public string Signature { get; set; } //text
    public string AuthorName { get; set; } //varchar(100)
    public DateTime CreateAt { get; set; }
    public Guid CreateBy { get; set; } // reference to UserID
    public DateTime UpdateAt { get; set; }
}
