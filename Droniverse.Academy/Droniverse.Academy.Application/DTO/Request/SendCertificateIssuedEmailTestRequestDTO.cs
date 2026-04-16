namespace Droniverse.Academy.Application.DTO.Request;

public class SendCertificateIssuedEmailTestRequestDTO
{
    public string CertificateImageUrl { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
