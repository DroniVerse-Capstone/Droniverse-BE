namespace Droniverse.Academy.Application.DTO.Request;

public class UpdateCertificateRequestDTO
{
    public string CertificateName { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string LogoCertificate { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Signature { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
}
