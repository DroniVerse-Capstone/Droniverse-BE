using DnsClient.Internal;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Identity.Application.HttpClients;

public class AcademyMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _distributedCache;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AcademyMicroserviceClient> _logger;
    private readonly IUserService _userService;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AcademyMicroserviceClient(
        HttpClient httpClient,
        IDistributedCache distributedCache,
        IHostEnvironment environment,
        ILogger<AcademyMicroserviceClient> logger,
        IUserService userService
    )
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
        _environment = environment;
        _logger = logger;
        _userService = userService;
    }

    public async Task<UserResponse> GetUserWithUserLevelMaxAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        try
        {
            var url = BuildAcademyPath($"/user/levels/max?userId={userId}");
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError(
                        "Academy service unavailable when getting user levels for user {UserId}",
                        userId);

                    throw new HttpRequestException(
                        "Academy service unavailable",
                        null,
                        HttpStatusCode.ServiceUnavailable);
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.LogWarning(
                        "Bad request when getting user levels for user {UserId}. Response: {Response}",
                        userId,
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    throw new HttpRequestException(
                        "Bad request when calling Academy API",
                        null,
                        HttpStatusCode.BadRequest);
                }

                _logger.LogError(
                    "Error when getting user levels for user {UserId}. Status: {StatusCode}, Response: {Response}",
                    userId,
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync(cancellationToken));

                throw new HttpRequestException(
                    $"Academy API error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var successResponse = await response.Content.ReadFromJsonAsync<SuccessResponse<IEnumerable<LevelMiniResponseDto>>>(_jsonOptions, cancellationToken);
            var result = successResponse?.Data;
            
            _logger.LogInformation(
                "Successfully retrieved user levels for user {UserId}. Levels count: {LevelsCount}",
                userId,
                result?.Count() ?? 0);

            UserResponse userResponse = await _userService.GetUserById(userId);
            return userResponse with { Level = result?.FirstOrDefault() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calling Academy API GetUserWithUserLevelMax for user {UserId}",
                userId);
            throw;
        }
    }

    private string BuildAcademyPath(string relativePath)
    {
        return $"{GetEndpoint().TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    private string GetEndpoint()
    {
        return _environment.IsDevelopment()
            ? "/academy"
            : "/api/academy";
    }
}


