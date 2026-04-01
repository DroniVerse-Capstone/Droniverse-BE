namespace Droniverse.Academy.Application.DTO.Request;

public class GrantUserCertificateRequestDTO
{
    public Guid CertificateID { get; set; }
    public Guid UserID { get; set; }
}
