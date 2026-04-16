using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal abstract class CommunityBaseClient
{
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected const int CacheAbsoluteExpirationSeconds = 300;
    protected const int CacheSlidingExpirationSeconds = 100;

    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly ICacheService CacheService;
    private readonly IHostEnvironment _environment;

    protected CommunityBaseClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
    {
        HttpClient = httpClient;
        Logger = logger;
        CacheService = cacheService;
        _environment = environment;
    }

    protected string BuildCommunityPath(string relativePath)
    {
        return $"{GetCommunityEndpoint().TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    protected string GetCacheKeyForRemainingQuantity(Guid clubId, Guid courseId)
    {
        return $"club:{clubId}:course:{courseId}:remaining-quantity";
    }

    private string GetCommunityEndpoint()
    {
        return _environment.IsDevelopment()
            ? "/community"
            : "/api/community";
    }
}
