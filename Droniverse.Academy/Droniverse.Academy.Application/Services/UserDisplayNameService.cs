using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.Services;

public class UserDisplayNameService : IUserDisplayNameService
{
    private readonly IdentityMicroserviceClient _identityClient;
    private readonly ILogger<UserDisplayNameService> _logger;

    public UserDisplayNameService(IdentityMicroserviceClient identityClient, ILogger<UserDisplayNameService> logger)
    {
        _identityClient = identityClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SimpleUserReponse>> GetListUserAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.ToDistinctValidIds();

        if (ids.Count == 0)
            return [];

        try
        {
            var users = await _identityClient.GetUsersBulk(ids);
            return users.Select(u => new SimpleUserReponse
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = AppHelper.GetFullName(u)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể lấy thông tin người dùng từ Identity service. Trả về danh sách rỗng.");
            return [];
        }
    }
}
