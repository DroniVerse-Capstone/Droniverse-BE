using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface IUserDisplayNameService
{
    Task<SimpleUserReponse?> ResolveUserDisplayNameAsync(Guid userId);
    Task<(SimpleUserReponse? Creator, SimpleUserReponse? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy);
}
