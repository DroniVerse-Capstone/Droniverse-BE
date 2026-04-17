using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface ICertificateService
{
    Task<CertificateResponseDTO> CreateCertificateAsync(Guid courseId, Guid versionId, CreateCertificateRequestDTO request, string imageUrl);

    Task<string> GetCourseVersionTitleVNAsync(Guid courseId, Guid versionId);

    Task<CertificateResponseDTO> GetCertificateAsync(Guid courseId, Guid versionId);

    Task<CertificateResponseDTO> UpdateCertificateAsync(Guid courseId, Guid versionId, Guid certificateId, UpdateCertificateRequestDTO request);

    Task DeleteCertificateAsync(Guid courseId, Guid versionId, Guid certificateId);

    Task<CertificateResponseDTO> GetCertificateByIdAsync(Guid certificateId);

    Task<IEnumerable<CertificateResponseDTO>> GetCertificatesByIdsAsync(IEnumerable<Guid> certificateIds);

    Task<IEnumerable<SimpleCertificateResponse>> GetCertificatesBulkAsync(IEnumerable<Guid> certificateIds, CancellationToken cancellationToken = default);

    Task<PaginationResult<IEnumerable<CertificateResponseDTO>>> GetAllCertificatesAsync(int pageIndex = 1, int pageSize = 50, string? search = null);
}
