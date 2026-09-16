using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Droniverse.Academy.Application.HttpClients.IdentityService;

internal sealed class UserClient : IdentityBaseClient
{
    public UserClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<SimpleUserReponse?> GetUserByUserID(Guid userId)
    {
        var userFromCache = await CacheService.GetAsync<UserResponse>(GetUserCacheKey(userId));
        if (userFromCache != null)
        {
            Logger.LogInformation($"User with id {userId} found in cache.");
            return new SimpleUserReponse
            {
                UserId = userFromCache.UserId,
                Email = userFromCache.Email,
                FullName = AppHelper.GetFullName(userFromCache)
            };
        }

        var httpResponseMsg = await HttpClient.GetAsync(BuildIdentityPath($"users/{userId}"));
        if (!httpResponseMsg.IsSuccessStatusCode)
        {
            if (httpResponseMsg.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Identity service unavailable.");
                return null;
            }

            if (httpResponseMsg.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning($"User with ID [{userId}] not found in Identity Microservice.");
                return null;
            }

            if (httpResponseMsg.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
            }

            throw new HttpRequestException($"Identity service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
        }

        var user = await httpResponseMsg.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        if (user == null)
        {
            throw new ArgumentException("Invalid userID");
        }

        await CacheService.SetAsync(
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
        {
            return [];
        }

        var distinctIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinctIds.Count == 0)
        {
            return [];
        }

        try
        {
            var usersById = new Dictionary<Guid, UserResponse>();
            var missingIds = new List<Guid>();

            foreach (var id in distinctIds)
            {
                var cached = await CacheService.GetAsync<UserResponse>(GetUserCacheKey(id));
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
                var response = await HttpClient.PostAsJsonAsync(
                    BuildIdentityPath("users/bulk"),
                    missingIds
                );

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        Logger.LogError("Identity service unavailable (bulk request).");
                        throw new HttpRequestException(
                            "Identity service unavailable",
                            null,
                            HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException(
                            "Bad request when calling Identity bulk API",
                            null,
                            HttpStatusCode.BadRequest);
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
                    await CacheService.SetAsync(
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
            Logger.LogError(ex, "Error calling Identity bulk API");
            throw;
        }
    }

    public async Task<IEnumerable<Guid>> GetUserIdsBySearchName(UserInfoSearchRequestDTO request)
    {
        var normalizedSearch = request.SearchName?.Trim().ToLowerInvariant() ?? string.Empty;
        var sortDirection = request.SortDirection ?? SortDirection.Asc;

        var cacheKey = $"users:searchIds:{normalizedSearch}:{sortDirection}";

        var cachedIds = await CacheService.GetAsync<IEnumerable<Guid>>(cacheKey);
        if (cachedIds != null)
        {
            Logger.LogInformation("Danh sách UserIds lấy từ cache.");
            return cachedIds;
        }

        var query =
            $"users/search-ids?SearchName={Uri.EscapeDataString(normalizedSearch)}" +
            $"&SortDirection={sortDirection}";

        var response = await HttpClient.GetAsync(BuildIdentityPath(query));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Identity service unavailable (search userIds).");
                throw new HttpRequestException(
                    "Identity service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException(
                    "Bad request when calling Identity search userIds API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            throw new HttpRequestException(
                $"Identity search userIds API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        var userIds = await response.Content.ReadFromJsonAsync<IEnumerable<Guid>>(JsonOptions) ?? [];

        try
        {
            await CacheService.SetAsync(
                cacheKey,
                userIds,
                UserCacheAbsoluteExpirationSeconds,
                UserCacheSlidingExpirationSeconds);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cache set failed (UserIds).");
        }

        return userIds;
    }

    public async Task<SearchUsersWithPaginationResponse> SearchUsersWithPaginationAsync(
        SearchUsersWithPaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        request ??= new SearchUsersWithPaginationRequest();

        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var pageIndex = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var userIdsKey = (request.UserIds != null && request.UserIds.Any())
            ? string.Join(",", request.UserIds.OrderBy(x => x))
            : "all";

        var cacheKey = $"users:search:{fullName}:{email}:{userIdsKey}:{pageIndex}:{pageSize}";

        var cached = await CacheService.GetAsync<SearchUsersWithPaginationResponse>(cacheKey);
        if (cached != null)
        {
            Logger.LogInformation("SearchUsersWithPagination lấy từ cache.");
            return cached;
        }

        var response = await HttpClient.PostAsJsonAsync(
            BuildIdentityPath("users/search-pagination"),
            new SearchUsersWithPaginationRequest
            {
                FullName = fullName,
                Email = email,
                UserIds = request.UserIds,
                CurrentPage = pageIndex,
                PageSize = pageSize
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Identity service unavailable (search users pagination).");
                throw new HttpRequestException(
                    "Identity service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.LogWarning(
                    "Bad request when calling search users pagination API. Response: {Response}",
                    content);

                throw new HttpRequestException(
                    "Bad request when calling Identity search pagination API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            Logger.LogError(
                "Error calling search users pagination API. Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                content);

            throw new HttpRequestException(
                $"Identity search pagination API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<SearchUsersWithPaginationResponse>(
            JsonOptions,
            cancellationToken);

        if (result == null)
        {
            throw new HttpRequestException("Invalid response from Identity search pagination API");
        }

        try
        {
            await CacheService.SetAsync(
                cacheKey,
                result,
                UserCacheAbsoluteExpirationSeconds,
                UserCacheSlidingExpirationSeconds);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cache set failed (SearchUsersWithPagination).");
        }

        return result;
    }
}
