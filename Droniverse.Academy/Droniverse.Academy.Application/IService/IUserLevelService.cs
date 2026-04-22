using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserLevelService
{
    Task<IEnumerable<LevelMiniResponse>> GetUserLevelsAsync(Guid userId);
    Task<IEnumerable<LevelMiniResponse>> GetMaxUserLevelsAsync(Guid userId);
}
