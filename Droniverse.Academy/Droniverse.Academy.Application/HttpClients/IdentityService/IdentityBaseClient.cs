using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.HttpClients.IdentityService;

internal abstract class IdentityBaseClient
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    protected const int UserCacheAbsoluteExpirationSeconds = 300;
    protected const int UserCacheSlidingExpirationSeconds = 100;

    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly ICacheService CacheService;
    private readonly IHostEnvironment _environment;

    protected IdentityBaseClient(
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

    protected string BuildIdentityPath(string relativePath)
    {
        return $"{GetEndpoint().TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    protected static string GetUserCacheKey(Guid userId)
    {
        return CacheKeysHelper.User(userId);
    }

    private string GetEndpoint()
    {
        return _environment.IsDevelopment()
            ? "/identity"
            : "/api/identity";
    }
}
