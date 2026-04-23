    using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.HttpClients;

public class IdentityMicroserviceClient
{
    private static readonly DistributedCacheEntryOptions UserCacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
        .SetSlidingExpiration(TimeSpan.FromSeconds(100));

    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache; //Redis Cache
    private readonly IHostEnvironment _environment;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) // parse string -> enum
        }
    };
    public IdentityMicroserviceClient(
        HttpClient httpClient,
        ILogger<IdentityMicroserviceClient> logger,
        IDistributedCache distributedCache,
        IHostEnvironment environment
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _environment = environment;
    }

    public async Task<UserResponse?> GetUserByUserID(Guid userId)
    {

        //Read from cache
        //key:value
        //userid:{object} ttl:30p
        string cacheKeyToRead = $"user:{userId}";
        string? cacheUser = await _distributedCache.GetStringAsync(cacheKeyToRead);
        if (cacheUser != null)
        {
            _logger.LogInformation($"User with id {userId} found in cache.");
            UserResponse? userFromCache = JsonSerializer.Deserialize<UserResponse>(cacheUser);
            return userFromCache ?? throw new NotFoundException($"User with ID {userId} not found in cache.");
        }

        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync(BuildIdentityPath($"users/{userId}"));

        if (!httpResponseMsg.IsSuccessStatusCode)
        {
            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Identity service unavailable.");
                return null;
            }

            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"User with ID [{userId}] not found in Identity Microservice.");
                return null;
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
            else
                throw new HttpRequestException($"Identity service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
        }
        UserResponse? user = await httpResponseMsg.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        if (user == null)
        {
            throw new ArgumentException("Invalid userID");
        }

        //Write to cache
        //key:value
        //string userKeyToWrite
        string userKeyToWrite = $"user:{userId}";
        string userCacheString = JsonSerializer.Serialize(user);
        await _distributedCache.SetStringAsync(userKeyToWrite, userCacheString, UserCacheOptions);
        return user;
    }

    public async Task<IEnumerable<UserResponse>> GetUsersBulk(IEnumerable<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return [];

        var distinctIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        var userDict = new Dictionary<Guid, UserResponse>();
        var missingIds = new List<Guid>();

        var cacheReadTasks = distinctIds.Select(async id =>
        {
            var cacheKey = $"user:{id}";
            var cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            _logger.LogInformation($"User with id [{id}] found in cache.");
            return (Id: id, CacheValue: cacheValue);
        });

        var cachedUsers = await Task.WhenAll(cacheReadTasks);

        foreach (var (id, cacheValue) in cachedUsers)
        {
            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                missingIds.Add(id);
                continue;
            }

            try
            {
                var cachedUser = JsonSerializer.Deserialize<UserResponse>(cacheValue);
                if (cachedUser == null)
                {
                    missingIds.Add(id);
                    continue;
                }

                userDict[id] = cachedUser;
            }
            catch
            {
                missingIds.Add(id);
            }
        }

        if (missingIds.Any())
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    BuildIdentityPath("users/bulk"),
                    //"/identity/users/bulk",
                    missingIds
                );

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogError("Identity service unavailable (bulk request).");
                        throw new HttpRequestException(
                            "Identity service unavailable",
                            null,
                            System.Net.HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException(
                            "Bad request when calling Identity bulk API",
                            null,
                            System.Net.HttpStatusCode.BadRequest);
                    }

                    throw new HttpRequestException(
                        $"Identity bulk API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var usersFromApi = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>(JsonOptions) ?? [];
                var cacheWriteTasks = new List<Task>();

                foreach (var user in usersFromApi)
                {
                    userDict[user.UserId] = user;
                    string userKeyToWrite = $"user:{user.UserId}";
                    string userCacheString = JsonSerializer.Serialize(user);
                    cacheWriteTasks.Add(_distributedCache.SetStringAsync(userKeyToWrite, userCacheString, UserCacheOptions));
                }

                if (cacheWriteTasks.Count > 0)
                    await Task.WhenAll(cacheWriteTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Identity bulk API");
                throw;
            }
        }

        return distinctIds
            .Where(id => userDict.ContainsKey(id))
            .Select(id => userDict[id]);
    }

    public async Task<IEnumerable<Guid>> GetUserIdsBySearchName(UserInfoSearchRequest request)
    {
        string normalizedSearch = request.SearchName?.Trim().ToLowerInvariant() ?? string.Empty;
        var sortDirection = request.SortDirection ?? SortDirection.Asc;

        string cacheKey = $"users:searchIds:{normalizedSearch}:{sortDirection}";
        string? cachedData = await _distributedCache.GetStringAsync(cacheKey);

        if (!string.IsNullOrWhiteSpace(cachedData))
        {
            try
            {
                var idsFromCache = JsonSerializer.Deserialize<IEnumerable<Guid>>(cachedData);
                if (idsFromCache != null)
                {
                    _logger.LogInformation("Danh sách UserIds lấy từ cache.");
                    return idsFromCache;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache deserialize failed (UserIds).");
            }
        }

        var query =
            $"users/search-ids?SearchName={Uri.EscapeDataString(normalizedSearch)}" +
            $"&SortDirection={sortDirection}";

        HttpResponseMessage response = await _httpClient.GetAsync(BuildIdentityPath(query));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Identity service unavailable (search userIds).");
                throw new HttpRequestException(
                    "Identity service unavailable",
                    null,
                    System.Net.HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    "Bad request when calling Identity search userIds API",
                    null,
                    System.Net.HttpStatusCode.BadRequest);
            }

            throw new HttpRequestException(
                $"Identity search userIds API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        var userIds = await response.Content.ReadFromJsonAsync<IEnumerable<Guid>>(JsonOptions) ?? [];

        try
        {
            string cacheValue = JsonSerializer.Serialize(userIds);
            await _distributedCache.SetStringAsync(cacheKey, cacheValue, UserCacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed (UserIds).");
        }

        return userIds;
    }

    private string BuildIdentityPath(string relativePath)
    {
        return $"{GetEndpoint().TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    private string GetEndpoint()
    {
        return _environment.IsDevelopment()
            ? "/identity"
            : "/api/identity";
    }

    /// <summary>
    /// Get service-to-service authentication token từ Identity service
    /// </summary>
    public async Task<string> GetServiceTokenAsync(string serviceId, string apiKey)
    {
        try
        {
            var requestBody = new { serviceId, apiKey };
            var response = await _httpClient.PostAsJsonAsync(
                BuildIdentityPath("auth/service-token"),
                requestBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get service token. Status: {response.StatusCode}");
                throw new HttpRequestException($"Service token request failed: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>(JsonOptions);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                throw new InvalidOperationException("Invalid service token response");
            }

            _logger.LogInformation($"Service token obtained for service: {serviceId}");
            return tokenResponse.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting service token");
            throw;
        }
    }
}

