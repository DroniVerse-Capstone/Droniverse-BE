using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class CertificateResponseDTO
{
    public Guid CertificateID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string CertificateNameVN { get; set; } = null!;
    public string CertificateNameEN { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string LogoCertificate { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Signature { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public SimpleUserReponse? Creator { get; set; }
    public DateTime UpdateAt { get; set; }
    public SimpleUserReponse? Updater { get; set; }
}
