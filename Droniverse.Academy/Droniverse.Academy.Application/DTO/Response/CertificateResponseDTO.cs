namespace Droniverse.Academy.Application.DTO.Response;

public class CertificateResponseDTO
{
    public Guid CertificateID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string CertificateName { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string LogoCertificate { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Signature { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime UpdateAt { get; set; }
    public Guid UpdateBy { get; set; }
}
