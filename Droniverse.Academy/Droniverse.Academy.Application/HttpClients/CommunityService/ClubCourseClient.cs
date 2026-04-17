using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal sealed class ClubCourseClient : CommunityBaseClient
{
    public ClubCourseClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<ClubCourseResponse> AddCourseToClub(Guid clubId, AddClubCourseRequestDto request)
    {
        var response = await HttpClient.PostAsync(BuildCommunityPath($"clubs/{clubId}/courses"), JsonContent.Create(request));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Community service unavailable when add course to club.");
                throw new HttpRequestException(
                    "Community service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.LogWarning("Bad request when when add course to club.", await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    "Bad request when calling Community API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            Logger.LogError("Error when add course to club.", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new HttpRequestException(
                $"Community API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        var clubCourse = await response.Content.ReadFromJsonAsync<ClubCourseResponse>();
        return clubCourse!;
    }

    public async Task<bool> ConsumeSlotForCodeAsync(
        Guid clubId,
        Guid courseId,
        int num,
        CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PostAsJsonAsync(
            BuildCommunityPath($"clubs/{clubId}/courses/{courseId}/consume"),
            new ChangeClubCourseSlotRequestDto { Quantity = num },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Community service unavailable when consuming slot for club {ClubId} and course {CourseId}.", clubId, courseId);
                throw new HttpRequestException(
                    "Community service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.LogWarning("Bad request when consuming slot for club {ClubId} and course {CourseId}. Response: {ResponseContent}", clubId, courseId, await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    "Bad request when calling Community API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            Logger.LogError("Error consuming slot for club {ClubId} and course {CourseId}. Status code: {StatusCode}, Response: {ResponseContent}", clubId, courseId, response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new HttpRequestException(
                $"Community API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        Logger.LogInformation("Đã gọi hàm consumeSlot qua bên Community. Consumed {Num} slots for club {ClubId} and course {CourseId}.", num, clubId, courseId);
        return true;
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
        var cachedValue = await CacheService.GetAsync<ClubCourseOwn>(cacheKey, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var response = await HttpClient.GetAsync(
            BuildCommunityPath($"clubs/{clubId}/courses/{courseId}"),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
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
        var wrappedResponse = JsonSerializer.Deserialize<CommunitySuccessResponse<ClubCourseOwn>>(json, JsonSerializerOptions);
        var remainingQuantityData = wrappedResponse?.Data
            ?? JsonSerializer.Deserialize<ClubCourseOwn>(json, JsonSerializerOptions);

        if (remainingQuantityData == null)
        {
            return null;
        }

        await CacheService.SetAsync(
            cacheKey,
            remainingQuantityData,
            CacheAbsoluteExpirationSeconds,
            CacheSlidingExpirationSeconds,
            cancellationToken);

        return remainingQuantityData;
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

        var response = await HttpClient.GetAsync(
            BuildCommunityPath($"clubs/{clubId}/courses/{courseId}"),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogError(
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
            JsonSerializerOptions,
            cancellationToken);

        return result;
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

        var response = await HttpClient.PostAsJsonAsync(
            BuildCommunityPath($"clubs/{clubId}/courses/{courseId}/consume-cross"),
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError(
                    "Community service unavailable when consuming slot (cross). ClubId: {ClubId}, CourseId: {CourseId}",
                    clubId, courseId);

                throw new HttpRequestException(
                    "Community service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.LogWarning(
                    "Bad request when consuming slot (cross). ClubId: {ClubId}, CourseId: {CourseId}, Response: {Response}",
                    clubId, courseId, content);

                throw new HttpRequestException(
                    "Bad request when calling Community API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                Logger.LogWarning(
                    "Not enough slots (cross). ClubId: {ClubId}, CourseId: {CourseId}, Response: {Response}",
                    clubId, courseId, content);

                throw new HttpRequestException(
                    "Số mã còn lại không đủ",
                    null,
                    HttpStatusCode.Conflict);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.LogWarning(
                    "ClubCourse not found (cross). ClubId: {ClubId}, CourseId: {CourseId}",
                    clubId, courseId);

                return null;
            }

            Logger.LogError(
                "Error consuming slot (cross). Status: {StatusCode}, Response: {Response}",
                response.StatusCode, content);

            throw new HttpRequestException(
                $"Community API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ClubCourseResponseDto>(
            JsonSerializerOptions,
            cancellationToken);

        Logger.LogInformation(
            "Consume slot (cross) success. ClubId: {ClubId}, CourseId: {CourseId}, Remaining: {Remaining}",
            clubId, courseId, result?.RemainingQuantity);

        return result;
    }

    private sealed class CommunitySuccessResponse<T>
    {
        public T? Data { get; set; }
    }
}
