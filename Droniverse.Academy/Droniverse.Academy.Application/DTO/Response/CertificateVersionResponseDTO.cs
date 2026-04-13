namespace Droniverse.Academy.Application.DTO.Response;

public class CertificateVersionResponseDTO
{
    public Guid CertificateID { get; set; }
    public string CertificateNameVN { get; set; } = null!;
    public string CertificateNameEN { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}
