using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserLevelService
{
    Task<IEnumerable<UserLevelResponse>> GetUserLevelsAsync(Guid userId);
    Task<IEnumerable<UserLevelResponse>> GetMaxUserLevelsAsync(Guid userId);
}
