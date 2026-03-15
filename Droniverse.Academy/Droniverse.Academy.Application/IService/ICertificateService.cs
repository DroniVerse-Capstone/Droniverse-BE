using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICertificateService
{
    Task<CertificateResponseDTO> CreateCertificateAsync(Guid courseId, Guid versionId, CreateCertificateRequestDTO request);

    Task<CertificateResponseDTO> GetCertificateAsync(Guid courseId, Guid versionId);

    Task<CertificateResponseDTO> UpdateCertificateAsync(Guid courseId, Guid versionId, Guid certificateId, UpdateCertificateRequestDTO request);

    Task DeleteCertificateAsync(Guid courseId, Guid versionId, Guid certificateId);

    Task<CertificateResponseDTO> GetCertificateByIdAsync(Guid certificateId);

    Task<IEnumerable<CertificateResponseDTO>> GetCertificatesByIdsAsync(IEnumerable<Guid> certificateIds);
}
