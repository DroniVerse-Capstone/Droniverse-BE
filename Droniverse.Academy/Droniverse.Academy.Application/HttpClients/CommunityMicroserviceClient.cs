using Droniverse.Academy.Application.DTO.Response;
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
    public class CommunityMicroserviceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CommunityMicroserviceClient> _logger;
        private readonly IDistributedCache _distributedCache; //Redis Cache

        public CommunityMicroserviceClient(HttpClient httpClient, ILogger<CommunityMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
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
                    var response = await _httpClient.PostAsJsonAsync("/community/categories/bulk", missingIds);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            _logger.LogError("Community service unavailable (bulk request).");
                            throw new HttpRequestException(
                                "Community service unavailable",
                                null,
                                System.Net.HttpStatusCode.ServiceUnavailable);
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            throw new HttpRequestException(
                                "Bad request when calling Community bulk API",
                                null,
                                System.Net.HttpStatusCode.BadRequest);
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
                _logger.LogError(ex, "Error calling Community bulk API");
                throw;
            }
        }
        private async Task<CategoryResponseDTO?> GetCategoryFromCacheAsync(Guid categoryId)
        {
            var categoryFromCache = await _distributedCache.GetStringAsync(GetCacheKeyForCategory(categoryId));
            if (categoryFromCache == null)
            {
                return null;
            }
            var category = JsonSerializer.Deserialize<CategoryResponseDTO>(categoryFromCache);
            if (category == null)
            {
                throw new Exception($"Failed to deserialize category with ID {categoryId} from cache.");
            }
            return category;
        }
        private async Task CacheCategory(CategoryResponseDTO category)
        {
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));
            var categoryJson = JsonSerializer.Serialize(category);
            await _distributedCache.SetStringAsync(GetCacheKeyForCategory(category.CategoryID), categoryJson, options);

        }

        private string GetCacheKeyForCategory(Guid categoryId)
        {
            return $"category:{categoryId}";
        }
    } 
}
