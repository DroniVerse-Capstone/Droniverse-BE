using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;

namespace Droniverse.Academy.Application.Services;

public class UserDisplayNameService : IUserDisplayNameService
{
    private readonly IdentityMicroserviceClient _identityClient;

    public UserDisplayNameService(IdentityMicroserviceClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async Task<string?> ResolveUserDisplayNameAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        var user = await _identityClient.GetUserByUserID(userId);
        if (user is null)
            return null;

        return $"{user.FirstName} {user.LastName}";
    }

    public async Task<(string? Creator, string? Updater)> ResolveCreatorUpdaterAsync(Guid createBy, Guid updateBy)
    {
        if (createBy == updateBy)
        {
            var fullName = await ResolveUserDisplayNameAsync(createBy);
            return (fullName, fullName);
        }

        var creatorTask = ResolveUserDisplayNameAsync(createBy);
        var updaterTask = ResolveUserDisplayNameAsync(updateBy);
        await Task.WhenAll(creatorTask, updaterTask);

        return (await creatorTask, await updaterTask);
    }
}
