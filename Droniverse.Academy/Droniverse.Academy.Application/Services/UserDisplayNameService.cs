using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.DTOs;

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

    public async Task<(SimpleUserReponse? Creator, SimpleUserReponse? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy)
    {
        if (createBy == updateBy)
        {
            var user = await ResolveUserDisplayNameAsync(createBy);
            return (user, user);
        }

        var creatorTask = ResolveUserDisplayNameAsync(createBy);
        var updaterTask = ResolveUserDisplayNameAsync(updateBy);
        await Task.WhenAll(creatorTask, updaterTask);

        return (await creatorTask, await updaterTask);
    }
}
