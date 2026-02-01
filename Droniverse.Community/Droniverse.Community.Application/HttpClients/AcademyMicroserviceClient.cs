
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Droniverse.Community.Application.HttpClients;

public class AcademyMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AcademyMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache; //Redis Cache
    public AcademyMicroserviceClient(
        HttpClient httpClient,
        ILogger<AcademyMicroserviceClient> logger,
        IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    
}

