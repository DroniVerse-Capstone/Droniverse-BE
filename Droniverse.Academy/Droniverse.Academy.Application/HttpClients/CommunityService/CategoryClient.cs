using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal sealed class CategoryClient : CommunityBaseClient
{
    public CategoryClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<IEnumerable<CategoryResponseDTO>> GetCategoriesBulk(IEnumerable<Guid> ids)
    {
        if (ids == null || !ids.Any())
        {
            return [];
        }

        var distinctIds = ids
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
        {
            return [];
        }

        try
        {
            var categoriesById = new Dictionary<Guid, CategoryResponseDTO>();
            var missingIds = new List<Guid>();

            foreach (var id in distinctIds)
            {
                var cached = await GetCategoryFromCacheAsync(id);
                if (cached != null)
                {
                    categoriesById[id] = cached;
                }
                else
                {
                    missingIds.Add(id);
                }
            }

            if (missingIds.Count > 0)
            {
                var response = await HttpClient.PostAsJsonAsync(BuildCommunityPath("categories/bulk"), missingIds);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        Logger.LogError("Community service unavailable (bulk request).");
                        throw new HttpRequestException(
                            "Community service unavailable",
                            null,
                            HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException(
                            "Bad request when calling Community bulk API",
                            null,
                            HttpStatusCode.BadRequest);
                    }

                    throw new HttpRequestException(
                        $"Community bulk API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var categoriesFromApi = await response.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>() ?? [];

                foreach (var category in categoriesFromApi)
                {
                    categoriesById[category.CategoryID] = category;
                    await CacheCategory(category);
                }
            }

            return distinctIds
                .Where(id => categoriesById.ContainsKey(id))
                .Select(id => categoriesById[id])
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling Community bulk API");
            throw;
        }
    }

    private async Task<CategoryResponseDTO?> GetCategoryFromCacheAsync(Guid categoryId)
    {
        return await CacheService.GetAsync<CategoryResponseDTO>(GetCacheKeyForCategory(categoryId));
    }

    private async Task CacheCategory(CategoryResponseDTO category)
    {
        await CacheService.SetAsync(
            GetCacheKeyForCategory(category.CategoryID),
            category,
            CacheAbsoluteExpirationSeconds,
            CacheSlidingExpirationSeconds);
    }

    private static string GetCacheKeyForCategory(Guid categoryId)
    {
        return CacheKeysHelper.Category(categoryId);
    }
}
