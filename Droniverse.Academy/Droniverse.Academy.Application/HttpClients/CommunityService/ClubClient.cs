using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal sealed class ClubClient : CommunityBaseClient
{
    public ClubClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<IEnumerable<DroneResponseDto>> GetMyClubsAsync()
    {
        var response = await HttpClient.GetAsync(BuildCommunityPath("clubs/myclub"));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Logger.LogError("Community service unavailable when get user's club");
                throw new HttpRequestException(
                    "Community service unavailable",
                    null,
                    HttpStatusCode.ServiceUnavailable);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.LogWarning("Bad request when get user's club", await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(
                    "Bad request when calling Community API",
                    null,
                    HttpStatusCode.BadRequest);
            }

            Logger.LogError("Error when get user's club", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new HttpRequestException(
                $"Community API error: {response.StatusCode}",
                null,
                response.StatusCode);
        }

        try
        {
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var dataElement))
            {
                Logger.LogWarning("No data property in response");
                return [];
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var clubs = JsonSerializer.Deserialize<IEnumerable<DroneResponseDto>>(dataElement.GetRawText(), options);
            return clubs ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deserializing clubs response");
            throw new HttpRequestException("Failed to deserialize club data", ex);
        }
    }

    public async Task<IEnumerable<SimpleClubResponse>> GetClubInfoBulkAsync(
        IEnumerable<Guid> clubIds,
        CancellationToken cancellationToken = default)
    {
        if (clubIds == null || !clubIds.Any())
        {
            return [];
        }

        var distinctIds = clubIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
        {
            return [];
        }

        try
        {
            var request = new GetClubSimpleInfoRequest
            {
                ClubIds = distinctIds
            };

            var response = await HttpClient.PostAsJsonAsync(
                BuildCommunityPath("clubs/clubIds/bulk"),
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Logger.LogError("Dịch vụ Community không khả dụng khi gọi API lấy danh sách câu lạc bộ.");
                    throw new HttpRequestException(
                        "Dịch vụ Community không khả dụng",
                        null,
                        HttpStatusCode.ServiceUnavailable);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Logger.LogWarning("Yêu cầu không hợp lệ khi gọi API danh sách câu lạc bộ. Phản hồi: {Response}",
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    throw new HttpRequestException(
                        "Yêu cầu không hợp lệ khi gọi Community API",
                        null,
                        HttpStatusCode.BadRequest);
                }

                Logger.LogError("Lỗi khi gọi API danh sách câu lạc bộ. Mã trạng thái: {StatusCode}, Phản hồi: {Response}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(cancellationToken));

                throw new HttpRequestException(
                    $"Lỗi từ Community API: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var clubs = await response.Content.ReadFromJsonAsync<List<SimpleClubResponse>>(JsonSerializerOptions, cancellationToken)
                        ?? [];

            var clubDict = clubs.ToDictionary(x => x.ClubId, x => x);

            return distinctIds
                .Where(id => clubDict.ContainsKey(id))
                .Select(id => clubDict[id])
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Lỗi khi gọi Community API lấy danh sách câu lạc bộ");
            throw;
        }
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
            var url = BuildCommunityPath($"clubs/{clubId}/participantIds?ParticipantStatus={status}");
            var response = await HttpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Logger.LogError(
                        "Community service unavailable when getting participantIds for club {ClubId}",
                        clubId);

                    throw new HttpRequestException(
                        "Community service unavailable",
                        null,
                        HttpStatusCode.ServiceUnavailable);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Logger.LogWarning(
                        "Bad request when getting participantIds for club {ClubId}. Response: {Response}",
                        clubId,
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    throw new HttpRequestException(
                        "Bad request when calling Community API",
                        null,
                        HttpStatusCode.BadRequest);
                }

                Logger.LogError(
                    "Error when getting participantIds for club {ClubId}. Status: {StatusCode}, Response: {Response}",
                    clubId,
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(cancellationToken));

                throw new HttpRequestException(
                    $"Community API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<GetClubParticipantsResponse>(
                JsonSerializerOptions,
                cancellationToken);

            return result?.participantIds ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error calling Community API GetClubParticipantIds for club {ClubId}",
                clubId);
            throw;
        }
    }

    public async Task<bool> CheckParticipantByClubAsync(
        Guid clubId,
        Guid userId,
        ParticipationStatus status = ParticipationStatus.ACTIVE,
        CancellationToken cancellationToken = default)
    {
        var url = BuildCommunityPath($"clubs/{clubId}/participants/{userId}?status={status}");
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await HttpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Logger.LogError(
            "CheckParticipant API failed. Status: {StatusCode}, Body: {Body}",
            response.StatusCode,
            content);

        throw new HttpRequestException(
            $"Community API error: {response.StatusCode}",
            null,
            response.StatusCode);
    }
}
 