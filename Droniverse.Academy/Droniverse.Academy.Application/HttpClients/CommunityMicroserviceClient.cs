using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
        private readonly IHostEnvironment _environment;
        private const int CacheAbsoluteExpirationSeconds = 300;
        private const int CacheSlidingExpirationSeconds = 100;

        public CommunityMicroserviceClient(HttpClient httpClient, ILogger<CommunityMicroserviceClient> logger, ICacheService cacheService, IHostEnvironment environment)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cacheService = cacheService;
            _environment = environment;
        }

        public async Task<ClubCourseResponse> AddCourseToClub(Guid clubId, AddClubCourseRequestDto request)
        {
            HttpResponseMessage response = await _httpClient.PostAsync(BuildCommunityPath($"clubs/{clubId}/courses"), JsonContent.Create(request));
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Community service unavailable when add course to club.");
                    throw new HttpRequestException(
                        "Community service unavailable",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Bad request when when add course to club.", await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException(
                        "Bad request when calling Community API",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }
                _logger.LogError("Error when add course to club.", response.StatusCode, await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            ClubCourseResponse? clubCourse = await response.Content.ReadFromJsonAsync<ClubCourseResponse>();
            return clubCourse;
        }

        public async Task<IEnumerable<ClubResponse>> GetMyClubsAsync()
        {

            //read cache from redis, if exist

            HttpResponseMessage response = await _httpClient.GetAsync(BuildCommunityPath("clubs/myclub"));
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Community service unavailable when get user's club");
                    throw new HttpRequestException(
                        "Community service unavailable",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Bad request when get user's club", await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException(
                        "Bad request when calling Community API",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }
                _logger.LogError("Error when get user's club", response.StatusCode, await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            try
            {
                // Parse as JsonElement để có control tốt hơn
                using var doc = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync());
                var root = doc.RootElement;

                if (!root.TryGetProperty("data", out var dataElement))
                {
                    _logger.LogWarning("No data property in response");
                    return [];
                }

                // Chuyển về JSON string và deserialize với custom options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() } // ← Quan trọng: handle enum as string
                };

                var clubs = JsonSerializer.Deserialize<IEnumerable<ClubResponse>>(
                    dataElement.GetRawText(),
                    options);

                return clubs ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing clubs response");
                throw new HttpRequestException("Failed to deserialize club data", ex);
            }

        }

        /// <summary>
        /// Khi club_member
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="courseId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<bool> ConsumeSlotForCodeAsync(
            Guid clubId,
            Guid courseId,
            int num,
            CancellationToken cancellationToken = default)
        {

            HttpResponseMessage? response = await _httpClient.PostAsJsonAsync(
                BuildCommunityPath($"clubs/{clubId}/courses/{courseId}/consume"), new ChangeClubCourseSlotRequestDto
                { Quantity = num },
    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Community service unavailable when consuming slot for club {ClubId} and course {CourseId}.", clubId, courseId);
                    throw new HttpRequestException(
                        "Community service unavailable",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning("Bad request when consuming slot for club {ClubId} and course {CourseId}. Response: {ResponseContent}", clubId, courseId, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException(
                        "Bad request when calling Community API",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }
                _logger.LogError("Error consuming slot for club {ClubId} and course {CourseId}. Status code: {StatusCode}, Response: {ResponseContent}", clubId, courseId, response.StatusCode, await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }
            _logger.LogInformation("Đã gọi hàm consumeSlot qua bên Community. Consumed {Num} slots for club {ClubId} and course {CourseId}.", num, clubId, courseId);
            return true;
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
                    var response = await _httpClient.PostAsJsonAsync(BuildCommunityPath("categories/bulk"), missingIds);

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
                BuildCommunityPath($"clubs/{clubId}/courses/{courseId}/remaining-quantity"),
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

                var response = await _httpClient.GetAsync(BuildCommunityPath($"products/reference/{referenceId}"), cancellationToken);

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
                _logger.LogError(ex, "Error calling Community product bulk API");
                throw;
            }
        }

        public async Task<IEnumerable<SimpleClubResponse>> GetClubInfoBulkAsync(
        IEnumerable<Guid> clubIds,
        CancellationToken cancellationToken = default)
        {
            if (clubIds == null && clubIds.Count() == 0)
                return [];

            var distinctIds = clubIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!distinctIds.Any())
                return [];

            try
            {
                var request = new GetClubSimpleInfoRequest
                {
                    ClubIds = distinctIds
                };

                var response = await _httpClient.PostAsJsonAsync(
                    BuildCommunityPath("clubs/clubIds/bulk"),
                    request,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogError("Dịch vụ Community không khả dụng khi gọi API lấy danh sách câu lạc bộ.");
                        throw new HttpRequestException(
                            "Dịch vụ Community không khả dụng",
                            null,
                            System.Net.HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning("Yêu cầu không hợp lệ khi gọi API danh sách câu lạc bộ. Phản hồi: {Response}",
                            await response.Content.ReadAsStringAsync(cancellationToken));

                        throw new HttpRequestException(
                            "Yêu cầu không hợp lệ khi gọi Community API",
                            null,
                            System.Net.HttpStatusCode.BadRequest);
                    }

                    _logger.LogError("Lỗi khi gọi API danh sách câu lạc bộ. Mã trạng thái: {StatusCode}, Phản hồi: {Response}",
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    throw new HttpRequestException(
                        $"Lỗi từ Community API: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var clubs = await response.Content.ReadFromJsonAsync<List<SimpleClubResponse>>(_jsonSerializerOptions, cancellationToken)
                            ?? [];

                // giữ đúng thứ tự input
                var clubDict = clubs.ToDictionary(x => x.ClubId, x => x);

                return distinctIds
                    .Where(id => clubDict.ContainsKey(id))
                    .Select(id => clubDict[id])
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Community API lấy danh sách câu lạc bộ");
                throw;
            }
        }

        public async Task<ClubCourseRemainingQuantityResponseDto?> GetRemainingQuantityRawAsync(
            Guid clubId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            if (clubId == Guid.Empty || courseId == Guid.Empty)
            {
                return null;
            }

            var response = await _httpClient.GetAsync(
                BuildCommunityPath($"clubs/{clubId}/courses/{courseId}"),
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogError(
                    "Error calling remaining quantity API for club {ClubId}, course {CourseId}. Status: {StatusCode}, Response: {Response}",
                    clubId,
                    courseId,
                    response.StatusCode,
                    errorContent);

                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<ClubCourseRemainingQuantityResponseDto>(
                _jsonSerializerOptions,
                cancellationToken);

            return result;
        }

        public async Task<List<Guid>> GetClubParticipantIdsAsync(
                    Guid clubId,
                    ParticipationStatus status = ParticipationStatus.ACTIVE,
                    CancellationToken cancellationToken = default)
        {
            if (clubId == Guid.Empty)
            {
                return [];
            }

            try
            {
                var url = BuildCommunityPath(
                    $"clubs/{clubId}/participantIds?ParticipantStatus={status}");

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogError(
                            "Community service unavailable when getting participantIds for club {ClubId}",
                            clubId);

                        throw new HttpRequestException(
                            "Community service unavailable",
                            null,
                            System.Net.HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning(
                            "Bad request when getting participantIds for club {ClubId}. Response: {Response}",
                            clubId,
                            await response.Content.ReadAsStringAsync(cancellationToken));

                        throw new HttpRequestException(
                            "Bad request when calling Community API",
                            null,
                            System.Net.HttpStatusCode.BadRequest);
                    }

                    _logger.LogError(
                        "Error when getting participantIds for club {ClubId}. Status: {StatusCode}, Response: {Response}",
                        clubId,
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    throw new HttpRequestException(
                        $"Community API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                // API của bạn trả về dạng:
                // { "participantIds": [...] }
                var result = await response.Content.ReadFromJsonAsync<GetClubParticipantsResponse>(
                    _jsonSerializerOptions,
                    cancellationToken);

                return result?.participantIds ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calling Community API GetClubParticipantIds for club {ClubId}",
                    clubId);
                throw;
            }
        }

        public async Task<ClubCourseResponseDto?> ConsumeSlotCrossAsync(
                Guid clubId,
                Guid courseId,
                int quantity = 1,
                CancellationToken cancellationToken = default)
        {
            var request = new ChangeClubCourseSlotRequestDto
            {
                Quantity = quantity
            };

            var response = await _httpClient.PostAsJsonAsync(
                BuildCommunityPath($"clubs/{clubId}/courses/{courseId}/consume-cross"),
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError(
                        "Community service unavailable when consuming slot (cross). ClubId: {ClubId}, CourseId: {CourseId}",
                        clubId, courseId);

                    throw new HttpRequestException(
                        "Community service unavailable",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning(
                        "Bad request when consuming slot (cross). ClubId: {ClubId}, CourseId: {CourseId}, Response: {Response}",
                        clubId, courseId, content);

                    throw new HttpRequestException(
                        "Bad request when calling Community API",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogWarning(
                        "Not enough slots (cross). ClubId: {ClubId}, CourseId: {CourseId}, Response: {Response}",
                        clubId, courseId, content);

                    throw new HttpRequestException(
                        "Số mã còn lại không đủ",
                        null,
                        System.Net.HttpStatusCode.Conflict);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(
                        "ClubCourse not found (cross). ClubId: {ClubId}, CourseId: {CourseId}",
                        clubId, courseId);

                    return null;
                }

                _logger.LogError(
                    "Error consuming slot (cross). Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, content);

                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<ClubCourseResponseDto>(
                _jsonSerializerOptions,
                cancellationToken);

            _logger.LogInformation(
                "Consume slot (cross) success. ClubId: {ClubId}, CourseId: {CourseId}, Remaining: {Remaining}",
                clubId, courseId, result?.RemainingQuantity);

            return result;
        }

        public async Task<bool> CheckParticipantByClubAsync(
    Guid clubId,
    Guid userId,
    ParticipationStatus status = ParticipationStatus.ACTIVE,
    CancellationToken cancellationToken = default)
        {
            var url = BuildCommunityPath(
                $"clubs/{clubId}/participants/{userId}?status={status}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "CheckParticipant API failed. Status: {StatusCode}, Body: {Body}",
                response.StatusCode,
                content);

            throw new HttpRequestException(
                $"Community API error: {response.StatusCode}",
                null,
                response.StatusCode);
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

        private string GetCacheKeyForCode(Guid codeId)
        {
            return $"code:{codeId}";
        }

        private string BuildCommunityPath(string relativePath)
        {
            return $"{GetCommunityEndpoint().TrimEnd('/')}/{relativePath.TrimStart('/')}";
        }

        private string GetCommunityEndpoint()
        {
            return _environment.IsDevelopment()
                ? "/community"
                : "/api/community";
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
