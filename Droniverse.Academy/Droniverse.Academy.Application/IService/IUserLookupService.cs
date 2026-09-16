using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface IUserLookupService
{
    Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Guid> userIds);
}
