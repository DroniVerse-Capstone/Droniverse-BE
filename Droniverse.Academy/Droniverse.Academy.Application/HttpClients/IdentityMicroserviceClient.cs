using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.HttpClients
{
    public class IdentityMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IdentityMicroserviceClient> _logger;
        private readonly ICacheService _cacheService;
        private readonly IHostEnvironment _environment;
        private const int UserCacheAbsoluteExpirationSeconds = 300;
        private const int UserCacheSlidingExpirationSeconds = 100;
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
            ICacheService cacheService,
            IHostEnvironment environment
            )
        {
            _httpClient = httpClient;
            _logger = logger;
            _cacheService = cacheService;
            _environment = environment;
        }

        public async Task<SimpleUserReponse?> GetUserByUserID(Guid userId)
        {
            var userFromCache = await _cacheService.GetAsync<UserResponse>(GetUserCacheKey(userId));
            if (userFromCache != null)
            {
                _logger.LogInformation($"User with id {userId} found in cache.");
                return new SimpleUserReponse
                {
                    UserId = userFromCache.UserId,
                    Email = userFromCache.Email,
                    FullName = AppHelper.GetFullName(userFromCache)
                };
            }

            //HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/users/{userId}");
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
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    //fallback data
                    throw new HttpRequestException($"Identity service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
                }
            }
            UserResponse? user = await httpResponseMsg.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
            if (user == null)
            {
                throw new ArgumentException("Invalid userID");

            }

            await _cacheService.SetAsync(
                GetUserCacheKey(user.UserId),
                user,
                UserCacheAbsoluteExpirationSeconds,
                UserCacheSlidingExpirationSeconds);

            return new SimpleUserReponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = AppHelper.GetFullName(user),
                AvatarUrl = user.ImageUrl
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

            if (distinctIds.Count == 0)
                return [];

            try
            {
                var usersById = new Dictionary<Guid, UserResponse>();
                var missingIds = new List<Guid>();

                foreach (var id in distinctIds)
                {
                    var cached = await _cacheService.GetAsync<UserResponse>(GetUserCacheKey(id));
                    if (cached != null)
                    {
                        usersById[id] = cached;
                    }
                    else
                    {
                        missingIds.Add(id);
                    }
                }

                if (missingIds.Count > 0)
                {
                    var response = await _httpClient.PostAsJsonAsync(
                        BuildIdentityPath("users/bulk"),
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

                    foreach (var user in usersFromApi)
                    {
                        usersById[user.UserId] = user;
                        await _cacheService.SetAsync(
                            GetUserCacheKey(user.UserId),
                            user,
                            UserCacheAbsoluteExpirationSeconds,
                            UserCacheSlidingExpirationSeconds);
                    }
                }

                return distinctIds
                    .Where(id => usersById.ContainsKey(id))
                    .Select(id => usersById[id])
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Identity bulk API");
                throw;
            }
        }

        public async Task<CertificateTemplateResponse?> GetCertificateTemplate()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(BuildIdentityPath("system-configs/certificate"));

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogError("Identity service unavailable when getting certificate template.");
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Certificate template not found.");
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException(
                            "Bad request when calling certificate template API",
                            null,
                            System.Net.HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        throw new HttpRequestException(
                            $"Identity service error: {response.StatusCode}",
                            null,
                            response.StatusCode);
                    }
                }

                var result = await response.Content
                    .ReadFromJsonAsync<CertificateTemplateResponse>(JsonOptions);

                if (result == null)
                {
                    throw new ArgumentException("Invalid certificate template response");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling certificate template API");
                throw;
            }
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

        private static string GetUserCacheKey(Guid userId) => $"user:{userId}";
    }
}
