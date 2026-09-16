using DnsClient.Internal;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AcademyMicroserviceClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AcademyMicroserviceClient(
        HttpClient httpClient,
        IHostEnvironment environment,
        ILogger<AcademyMicroserviceClient> logger
    )
    {
        _httpClient = httpClient;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IEnumerable<UserLevelResponseDto>?> GetUserLevelsAsync(Guid userId)
    {
        var url = BuildAcademyPath($"/user/levels?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        return successResponse?.Data ?? Enumerable.Empty<UserLevelResponseDto>();
    }

    public async Task<IEnumerable<UserLevelResponseDto>?> GetUserLevelMaxAsync(Guid userId)
    {
        var url = BuildAcademyPath($"/user/levels/max?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        return successResponse?.Data ?? Enumerable.Empty<UserLevelResponseDto>();
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


