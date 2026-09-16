using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserLevelService
{
    Task<IEnumerable<UserLevelResponse>> GetUserLevelsAsync(Guid userId);
    Task<IEnumerable<Guid>> GetUserLevelIdsAsync(Guid userId);
    Task<IEnumerable<UserLevelResponse>> GetMaxUserLevelsAsync(Guid userId);
    Task<bool> CreateLevelOneIfFirstEnrollmentAsync(Guid userId, Guid courseVersionId);
    Task<bool> CanUserUpgradeAsync(Guid userId, Guid droneId);
    Task<bool> UpgradeUserLevelAsync(Guid userId, Guid droneId);
}
