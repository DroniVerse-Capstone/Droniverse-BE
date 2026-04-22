using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;

namespace Droniverse.Identity.Application.HttpClients;

public class AcademyMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _distributedCache;

    public AcademyMicroserviceClient(HttpClient httpClient, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
    }

    //public async Task<UserResponse> GetUserWithUserLevelAsync(Guid userId)
    //{
    //    await _httpClient.GetAsync($"/api/users/{userId}");
    //}
}

