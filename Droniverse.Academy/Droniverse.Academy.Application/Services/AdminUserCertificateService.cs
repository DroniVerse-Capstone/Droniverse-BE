using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.Services;

public class AdminUserCertificateService : IAdminUserCertificateService
{
    private readonly IUserCertificateService _userCertificateService;

    public AdminUserCertificateService(IUserCertificateService userCertificateService)
    {
        _userCertificateService = userCertificateService;
    }

    public Task GrantCertificateToUserAsync(GrantUserCertificateRequestDTO request)
    {
        return _userCertificateService.GrantCertificateToUserAsync(request);
    }

    public Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUserCertificatesAsync(Guid userId, int pageIndex = 1, int pageSize = 50, UserCertificateStatus? status = null)
    {
        return _userCertificateService.GetUserCertificatesAsync(userId, pageIndex, pageSize, status);
    }

    public Task<UserCertificateResponseDTO> GetUserCertificateAsync(Guid userId, Guid certificateId)
    {
        return _userCertificateService.GetUserCertificateAsync(userId, certificateId);
    }

    public Task<PaginationResult<IEnumerable<UserCertificateResponseDTO>>> GetUsersByCertificateAsync(Guid certificateId, int pageIndex = 1, int pageSize = 50, UserCertificateStatus? status = null)
    {
        return _userCertificateService.GetUsersByCertificateAsync(certificateId, pageIndex, pageSize, status);
    }

    public Task RevokeUserCertificateAsync(Guid userId, Guid certificateId)
    {
        return _userCertificateService.RevokeUserCertificateAsync(userId, certificateId);
    }
}
