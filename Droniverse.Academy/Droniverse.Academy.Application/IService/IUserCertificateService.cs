using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserCertificateService
{
    Task GrantCertificateToUserAsync(GrantUserCertificateRequestDTO request);

    Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetMyCertificatesAsync(int pageIndex = 1, int pageSize = 50, UserCertificateStatus? status = null);

    Task<UserCertificateResponseDTO> GetMyCertificateAsync(Guid certificateId);

    Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUserCertificatesAsync(Guid userId, int pageIndex = 1, int pageSize = 50, UserCertificateStatus? status = null);

    Task<UserCertificateResponseDTO> GetUserCertificateAsync(Guid userId, Guid certificateId);

    Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUsersByCertificateAsync(Guid certificateId, int pageIndex = 1, int pageSize = 50, UserCertificateStatus? status = null);

    Task RevokeUserCertificateAsync(Guid userId, Guid certificateId);
}
