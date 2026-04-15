using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICertificateCreationService
{
    Task<CertificateResponseDTO> CreateCertificateWithGeneratedImageAsync(
        Guid courseId,
        Guid versionId,
        CreateCertificateRequestDTO request,
        CancellationToken cancellationToken = default);
}
