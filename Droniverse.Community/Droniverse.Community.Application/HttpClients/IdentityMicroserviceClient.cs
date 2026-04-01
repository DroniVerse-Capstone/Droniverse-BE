using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Droniverse.Community.Application.HttpClients;

public class IdentityMicroserviceClient
{
    private static readonly DistributedCacheEntryOptions UserCacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
        .SetSlidingExpiration(TimeSpan.FromSeconds(100));

    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache; //Redis Cache
    public IdentityMicroserviceClient(
        HttpClient httpClient,
        ILogger<IdentityMicroserviceClient> logger,
        IDistributedCache distributedCache
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
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

        //HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/identity/users/{userId}");
        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/identity/users/{userId}");

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
        UserResponse? user = await httpResponseMsg.Content.ReadFromJsonAsync<UserResponse>();
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
                    //"/api/identity/users/bulk",
                    "/identity/users/bulk",
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

                var usersFromApi = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>() ?? [];
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
}

