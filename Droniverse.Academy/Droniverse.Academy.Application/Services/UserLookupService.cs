using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.Services;

public class UserLookupService : IUserLookupService
{
    private readonly IUserDisplayNameService _userDisplayNameService;

    public UserLookupService(IUserDisplayNameService userDisplayNameService)
    {
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.ToDistinctValidIds();

        if (ids.Count == 0)
            return [];

        var users = await _userDisplayNameService.GetListUserAsync(ids);
        var lookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        foreach (var userId in ids)
        {
            lookup.TryAdd(userId, null);
        }

        return lookup;
    }
}
