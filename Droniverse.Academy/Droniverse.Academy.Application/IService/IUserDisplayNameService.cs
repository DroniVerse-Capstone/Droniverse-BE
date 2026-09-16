using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface IUserDisplayNameService
{
    Task<IReadOnlyList<SimpleUserReponse>> GetListUserAsync(IEnumerable<Guid> userIds);
}
