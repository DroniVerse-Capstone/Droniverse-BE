using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.HttpClients
{
    public class IdentityMicroserviceClient
    {
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

        public async Task<SimpleUserReponse?> GetUserByUserID(Guid userId)
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
                if (userFromCache == null)
                {
                    throw new NotFoundException($"User with ID {userId} not found in cache.");
                }
                return new SimpleUserReponse
                {
                    UserId = userFromCache.UserId,
                    Email = userFromCache.Email,
                    FullName = AppHelper.GetFullName(userFromCache)
                };
            }

            //HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/users/{userId}");
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
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    //fallback data
                    throw new HttpRequestException($"Identity service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
                }
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
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));
            await _distributedCache.SetStringAsync(userKeyToWrite, userCacheString, options);

            return new SimpleUserReponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = AppHelper.GetFullName(user)

            };
        }

        public async Task<IEnumerable<UserResponse>> GetUsersBulk(IEnumerable<Guid> userIds)
        {
            if (userIds == null || !userIds.Any())
                return [];

            var distinctIds = userIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            try
            {
                //var response = await _httpClient.PostAsJsonAsync(
                //    "/api/users/bulk",
                //    distinctIds
                //);
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/users/bulk",
                    distinctIds
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

                var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();

                return users ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Identity bulk API");
                throw;
            }
        }
    }
}
