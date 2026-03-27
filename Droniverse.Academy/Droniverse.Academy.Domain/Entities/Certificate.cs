using Droniverse.Academy.Domain.Entities;

public class Certificate
{
    public Guid CertificateID { get; set; }          // PK
    public Guid CourseVersionID { get; set; }        // FK -> CourseVersion
    public string CertificateName { get; set; }      // varchar(100)
    public string ImageUrl { get; set; }              // text
    public string LogoCertificate { get; set; }       // text
    public string Description { get; set; }           // text
    public string Signature { get; set; }             // text
    public string AuthorName { get; set; }            // varchar(100)
    public DateTime CreateAt { get; private set; }
    public Guid CreateBy { get; private set; }                // FK -> User
    public Guid UpdateBy { get; private set; }                // FK -> User
    public DateTime UpdateAt { get; private set; }

    // Navigation
    public CourseVersion CourseVersion { get; set; }
    public ICollection<UserCertificate> UserCertificates { get; set; }

    public void SetAuditOnCreate(Guid userId, DateTime now)
    {
        CreateBy = userId;
        CreateAt = now;
        UpdateBy = userId;
        UpdateAt = now;
    }

    public void SetAuditOnUpdate(Guid userId, DateTime now)
    {
        UpdateBy = userId;
        UpdateAt = now;
    }
}
