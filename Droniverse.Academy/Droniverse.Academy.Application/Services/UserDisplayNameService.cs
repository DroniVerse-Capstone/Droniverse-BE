using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Helpers;

namespace Droniverse.Academy.Application.Services;

public class UserDisplayNameService : IUserDisplayNameService
{
    private readonly IdentityMicroserviceClient _identityClient;

    public UserDisplayNameService(IdentityMicroserviceClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async Task<SimpleUserReponse?> ResolveUserDisplayNameAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        return await _identityClient.GetUserByUserID(userId);
    }

    public async Task<IReadOnlyDictionary<Guid, SimpleUserReponse?>> ResolveUsersDisplayNameAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return new Dictionary<Guid, SimpleUserReponse?>();

        var users = await _identityClient.GetUsersBulk(ids);
        var lookup = users.ToDictionary(
            u => u.UserId,
            u => (SimpleUserReponse?)new SimpleUserReponse
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = AppHelper.GetFullName(u)
            });

        foreach (var id in ids)
        {
            if (!lookup.ContainsKey(id))
            {
                lookup[id] = null;
            }
        }

        return lookup;
    }

    public async Task<(SimpleUserReponse? Creator, SimpleUserReponse? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy)
    {
        if (createBy == updateBy)
        {
            var user = await ResolveUserDisplayNameAsync(createBy);
            return (user, user);
        }

        var users = await ResolveUsersDisplayNameAsync(new[] { createBy, updateBy });
        users.TryGetValue(createBy, out var creator);
        users.TryGetValue(updateBy, out var updater);

        return (creator, updater);
    }
}
