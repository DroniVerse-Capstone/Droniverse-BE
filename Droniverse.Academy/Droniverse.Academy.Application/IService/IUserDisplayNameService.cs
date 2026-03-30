using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface IUserDisplayNameService
{
    Task<SimpleUserReponse?> ResolveUserDisplayNameAsync(Guid userId);
    Task<IReadOnlyDictionary<Guid, SimpleUserReponse?>> ResolveUsersDisplayNameAsync(IEnumerable<Guid> userIds);
    Task<(SimpleUserReponse? Creator, SimpleUserReponse? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy);
}
