namespace Droniverse.Academy.Application.DTO.Request;

public class GetCertificatesByIdsRequestDTO
{
    public List<Guid> CertificateIds { get; set; } = [];
}
