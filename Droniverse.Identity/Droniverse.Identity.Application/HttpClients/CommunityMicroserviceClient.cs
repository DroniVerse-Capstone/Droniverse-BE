using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;

namespace Droniverse.Identity.Application.HttpClients;

public class CommunityMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _distributedCache;

    public CommunityMicroserviceClient(HttpClient httpClient, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
    }


}

