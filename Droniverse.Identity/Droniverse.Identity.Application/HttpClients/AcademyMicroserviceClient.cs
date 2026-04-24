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

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private static string GetUserLevelCacheKey(Guid userId) => $"userLevel:{userId}";
    private static string GetUserLevelMaxCacheKey(Guid userId) => $"userLevelMax:{userId}";

    public AcademyMicroserviceClient(
        HttpClient httpClient,
        IDistributedCache distributedCache,
        IHostEnvironment environment,
        ILogger<AcademyMicroserviceClient> logger
    )
    {
        _httpClient = httpClient;
        _distributedCache = distributedCache;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IEnumerable<UserLevelResponseDto>?> GetUserLevelsAsync(Guid userId)
    {
        string userLevelCacheKey = GetUserLevelCacheKey(userId);
        string? cachedUserLevel = await _distributedCache.GetStringAsync(userLevelCacheKey);
        // 1. Đọc từ cache
        if (!string.IsNullOrEmpty(cachedUserLevel))
        {
            try
            {
                var cachedData = JsonSerializer.Deserialize<IEnumerable<UserLevelResponseDto>>(cachedUserLevel, _jsonOptions);
                if (cachedData != null) return cachedData;
            }
            catch (Exception ex)
            {
                // Bắt lỗi Deserialize phòng trường hợp cấu trúc JSON cũ bị sai trong Redis
                _logger.LogWarning(ex, $"Lỗi parse cache cho userLevel của userId: {userId}. Sẽ tiến hành gọi lại API.");
            }
        }

        // 2. Gọi API bên Academy
        var url = BuildAcademyPath($"/user/levels?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        var resultData = successResponse?.Data ?? Enumerable.Empty<UserLevelResponseDto>();

        // 3. Ghi vào Cache (SỬA LỖI TẠI ĐÂY)
        if (resultData.Any()) // Chỉ lưu cache nếu thực sự có dữ liệu
        {
            try
            {
                await _distributedCache.SetStringAsync(
                    userLevelCacheKey,
                    JsonSerializer.Serialize(resultData, _jsonOptions), // Đã bỏ FirstOrDefault()
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Set cache cho userLevel thất bại với userId: {userId}");
            }
        }

        return resultData;
    }

    public async Task<IEnumerable<UserLevelResponseDto>?> GetUserLevelMaxAsync(Guid userId)
    {
        string userLevelMaxCacheKey = GetUserLevelMaxCacheKey(userId);
        string? cachedUserLevelMax = await _distributedCache.GetStringAsync(userLevelMaxCacheKey);

        // 1. Đọc từ cache (Thêm try-catch để an toàn giống hàm GetUserLevelsAsync)
        if (!string.IsNullOrEmpty(cachedUserLevelMax))
        {
            try
            {
                var cachedData = JsonSerializer.Deserialize<IEnumerable<UserLevelResponseDto>>(cachedUserLevelMax, _jsonOptions);
                if (cachedData != null) return cachedData;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Lỗi parse cache cho userLevelMax của userId: {userId}. Sẽ gọi lại API.");
            }
        }

        // 2. Gọi API bên Academy
        var url = BuildAcademyPath($"/user/levels/max?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        var resultData = successResponse?.Data ?? Enumerable.Empty<UserLevelResponseDto>();

        // 3. Ghi vào Cache
        if (resultData.Any()) // Chỉ lưu nếu có dữ liệu
        {
            try
            {
                await _distributedCache.SetStringAsync(
                    userLevelMaxCacheKey,
                    JsonSerializer.Serialize(resultData, _jsonOptions),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Set cache cho userLevelMax thất bại với userId: {userId}");
            }
        }

        return resultData;
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


