using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal sealed class ProductClient : CommunityBaseClient
{
    public ProductClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
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

            var response = await HttpClient.GetAsync(BuildCommunityPath($"products/reference/{referenceId}"), cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
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
            Logger.LogError(ex, "Error calling Community product API for referenceId {ReferenceId}", referenceId);
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

            var response = await HttpClient.PostAsJsonAsync(
                BuildCommunityPath("products/reference/bulk"),
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
            Logger.LogError(ex, "Error calling Community product bulk API");
            throw;
        }
    }

    private async Task<ProductMiniResponseDTO?> GetProductFromCacheAsync(Guid referenceId, CancellationToken cancellationToken)
    {
        return await CacheService.GetAsync<ProductMiniResponseDTO>(GetCacheKeyForProduct(referenceId), cancellationToken);
    }

    private async Task CacheProductAsync(Guid productId, ProductMiniResponseDTO product, CancellationToken cancellationToken)
    {
        await CacheService.SetAsync(
            GetCacheKeyForProduct(productId),
            product,
            CacheAbsoluteExpirationSeconds,
            CacheSlidingExpirationSeconds,
            cancellationToken);
    }

    private static string GetCacheKeyForProduct(Guid referenceId)
    {
        return CacheKeysHelper.ProductReference(referenceId);
    }
}
