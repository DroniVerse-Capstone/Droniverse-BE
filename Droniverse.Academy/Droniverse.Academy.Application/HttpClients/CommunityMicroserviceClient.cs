using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.HttpClients
{
    public class CommunityMicroserviceClient
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<CommunityMicroserviceClient> _logger;
        private readonly ICacheService _cacheService;
        private const int CacheAbsoluteExpirationSeconds = 300;
        private const int CacheSlidingExpirationSeconds = 100;

        public CommunityMicroserviceClient(HttpClient httpClient, ILogger<CommunityMicroserviceClient> logger, ICacheService cacheService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cacheService = cacheService;
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

        public async Task<ClubCourseOwn?> GetRemainingQuantityAsync(
            Guid clubId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            if (clubId == Guid.Empty || courseId == Guid.Empty)
            {
                return null;
            }

            var cacheKey = GetCacheKeyForRemainingQuantity(clubId, courseId);
            var cachedValue = await _cacheService.GetAsync<ClubCourseOwn>(cacheKey, cancellationToken);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var response = await _httpClient.GetAsync(
                $"/community/clubs/{clubId}/courses/{courseId}/remaining-quantity",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Community remaining quantity API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var wrappedResponse = JsonSerializer.Deserialize<CommunitySuccessResponse<ClubCourseOwn>>(json, _jsonSerializerOptions);
            var remainingQuantityData = wrappedResponse?.Data
                ?? JsonSerializer.Deserialize<ClubCourseOwn>(json, _jsonSerializerOptions);

            if (remainingQuantityData == null)
            {
                return null;
            }

            await _cacheService.SetAsync(
                cacheKey,
                remainingQuantityData,
                CacheAbsoluteExpirationSeconds,
                CacheSlidingExpirationSeconds,
                cancellationToken);

            return remainingQuantityData;
        }

        public async Task<ProductMiniResponseDTO?> GetProductByReferenceIdAsync(
            Guid referenceId,
            CancellationToken cancellationToken = default)
        {
            if (referenceId == Guid.Empty)
            {
                return null;
            }

            try
            {
                var cachedProduct = await GetProductFromCacheAsync(referenceId, cancellationToken);
                if (cachedProduct != null)
                {
                    return cachedProduct;
                }

                var response = await _httpClient.GetAsync($"/community/products/reference/{referenceId}", cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Community product API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var product = await response.Content.ReadFromJsonAsync<ProductMiniResponseDTO>(cancellationToken);
                if (product != null)
                {
                    await CacheProductAsync(referenceId, product, cancellationToken);
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Community product API for referenceId {ReferenceId}", referenceId);
                throw;
            }
        }

        public async Task<IEnumerable<ProductMiniResponseDTO>> GetProductsBulkByReferenceIdsAsync(
            IEnumerable<Guid> referenceIds,
            CancellationToken cancellationToken = default)
        {
            if (referenceIds == null)
            {
                return [];
            }

            var distinctIds = referenceIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (distinctIds.Count == 0)
            {
                return [];
            }

            try
            {
                var productsByReferenceId = new Dictionary<Guid, ProductMiniResponseDTO>();
                var missingIds = new List<Guid>();

                foreach (var referenceId in distinctIds)
                {
                    var cachedProduct = await GetProductFromCacheAsync(referenceId, cancellationToken);
                    if (cachedProduct != null)
                    {
                        productsByReferenceId[referenceId] = cachedProduct;
                    }
                    else
                    {
                        missingIds.Add(referenceId);
                    }
                }

                if (missingIds.Count == 0)
                {
                    return distinctIds
                        .Where(id => productsByReferenceId.ContainsKey(id))
                        .Select(id => productsByReferenceId[id])
                        .ToList();
                }

                var response = await _httpClient.PostAsJsonAsync(
                    "/community/products/reference/bulk",
                    missingIds,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Community product bulk API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var productsFromApi = await response.Content.ReadFromJsonAsync<List<ProductMiniResponseDTO>>(cancellationToken)
                    ?? [];

                foreach (var product in productsFromApi)
                {
                    if (product.ReferenceId == Guid.Empty)
                    {
                        continue;
                    }

                    productsByReferenceId[product.ReferenceId] = product;
                    await CacheProductAsync(product.ProductId, product, cancellationToken);
                }

                return distinctIds
                    .Where(id => productsByReferenceId.ContainsKey(id))
                    .Select(id => productsByReferenceId[id])
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Community product bulk API");
                throw;
            }
        }

        private async Task<ProductMiniResponseDTO?> GetProductFromCacheAsync(Guid referenceId, CancellationToken cancellationToken)
        {
            return await _cacheService.GetAsync<ProductMiniResponseDTO>(GetCacheKeyForProduct(referenceId), cancellationToken);
        }

        private async Task CacheProductAsync(Guid productId, ProductMiniResponseDTO product, CancellationToken cancellationToken)
        {
            await _cacheService.SetAsync(
                GetCacheKeyForProduct(productId),
                product,
                CacheAbsoluteExpirationSeconds,
                CacheSlidingExpirationSeconds,
                cancellationToken);
        }

        private async Task<CategoryResponseDTO?> GetCategoryFromCacheAsync(Guid categoryId)
        {
            return await _cacheService.GetAsync<CategoryResponseDTO>(GetCacheKeyForCategory(categoryId));
        }

        private async Task CacheCategory(CategoryResponseDTO category)
        {
            await _cacheService.SetAsync(
                GetCacheKeyForCategory(category.CategoryID),
                category,
                CacheAbsoluteExpirationSeconds,
                CacheSlidingExpirationSeconds);
        }

        private string GetCacheKeyForCategory(Guid categoryId)
        {
            return $"category:{categoryId}";
        }

        private string GetCacheKeyForProduct(Guid referenceId)
        {
            return $"product:reference:{referenceId}";
        }

        private string GetCacheKeyForRemainingQuantity(Guid clubId, Guid courseId)
        {
            return $"club:{clubId}:course:{courseId}:remaining-quantity";
        }

        private sealed class CommunitySuccessResponse<T>
        {
            public T? Data { get; set; }
        }


    } 
}
