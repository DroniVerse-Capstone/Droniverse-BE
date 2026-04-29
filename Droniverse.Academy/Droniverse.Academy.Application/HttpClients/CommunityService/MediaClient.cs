using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Droniverse.Academy.Application.HttpClients.CommunityService;

internal sealed class MediaClient : CommunityBaseClient
{
    public MediaClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<IEnumerable<Droniverse.Shared.DTOs.Response.MediaMiniResponse>> GetMiniResponse(
        IEnumerable<Guid>? mediaIds,
        CancellationToken cancellationToken = default)
    {
        if (mediaIds == null)
        {
            return [];
        }

        var distinctIds = mediaIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinctIds.Count == 0)
        {
            return [];
        }

        try
        {
            var queryString = string.Join(
                "&",
                distinctIds.Select(id => $"mediaIds={Uri.EscapeDataString(id.ToString())}"));

            var response = await HttpClient.GetAsync(
                BuildCommunityPath($"media/mini?{queryString}"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Community media mini API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var medias = JsonSerializer.Deserialize<List<MediaMiniResponse>>(json, JsonSerializerOptions);
            return medias ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling Community media mini API");
            throw;
        }
    }
}