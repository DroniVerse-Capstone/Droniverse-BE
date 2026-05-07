using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.HttpClients;

public class AcademyMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AcademyMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache; //Redis Cache
    private readonly IHostEnvironment _environment;
    private readonly ICurrentUserService _currentUserService;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly IConfiguration _configuration;
    private string _cachedServiceToken = string.Empty;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private static readonly DistributedCacheEntryOptions CourseCacheOptions =
    new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
        .SetSlidingExpiration(TimeSpan.FromMinutes(2));
    private static readonly DistributedCacheEntryOptions LabCacheOptions =
    new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
    private static readonly DistributedCacheEntryOptions HotCourseCacheOptions =
    new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3))
        .SetSlidingExpiration(TimeSpan.FromMinutes(1));
    private static readonly DistributedCacheEntryOptions DroneCacheOptions =
        new DistributedCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private static string GetUserLevelCacheKey(Guid userId) => $"userLevel:{userId}";
    private static string GetUserLevelMaxCacheKey(Guid userId) => $"userLevelMax:{userId}";
    private readonly IClock _clock;
    public AcademyMicroserviceClient(
        HttpClient httpClient,
        ILogger<AcademyMicroserviceClient> logger,
        IDistributedCache distributedCache,
        IHostEnvironment environment,
        ICurrentUserService currentUserService,
        IdentityMicroserviceClient identityMicroserviceClient,
        IConfiguration configuration,
        IClock clock
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _environment = environment;
        _currentUserService = currentUserService;
        _identityMicroserviceClient = identityMicroserviceClient;
        _configuration = configuration;
        _clock = clock;
    }

    public async Task<UserLevelResponseDto?> GetUserLevelsAsync(Guid userId)
    {
        string userLevelCacheKey = GetUserLevelCacheKey(userId);
        string? cachedUserLevel = await _distributedCache.GetStringAsync(userLevelCacheKey);
        if (!string.IsNullOrEmpty(cachedUserLevel))
        {
            return JsonSerializer.Deserialize<UserLevelResponseDto>(cachedUserLevel, _jsonOptions);
        }

        var url = BuildAcademyPath($"/user/levels?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        try
        {
            await _distributedCache.SetStringAsync(
            userLevelCacheKey,
            JsonSerializer.Serialize(successResponse?.Data?.FirstOrDefault(), _jsonOptions),
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


        return successResponse?.Data?.FirstOrDefault();
    }

    public async Task<UserLevelResponseDto?> GetUserLevelMaxAsync(Guid userId)  
    {
        string userLevelMaxCacheKey = GetUserLevelMaxCacheKey(userId);
        string? cachedUserLevelMax = await _distributedCache.GetStringAsync(userLevelMaxCacheKey);
        if (!string.IsNullOrEmpty(cachedUserLevelMax))
        {
            return JsonSerializer.Deserialize<UserLevelResponseDto>(cachedUserLevelMax, _jsonOptions);
        }

        var url = BuildAcademyPath($"/user/levels/max?userId={userId}");
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var successResponse = await response.Content
            .ReadFromJsonAsync<SuccessResponse<IEnumerable<UserLevelResponseDto>>>(_jsonOptions);

        try
        {
            await _distributedCache.SetStringAsync(
            userLevelMaxCacheKey,
            JsonSerializer.Serialize(successResponse?.Data?.FirstOrDefault(), _jsonOptions),
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

        return successResponse?.Data?.FirstOrDefault();
    }

    public async Task LimitUserAccessAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("UserId không hợp lệ.");

        var serviceToken = await GetValidServiceTokenAsync();

        var requestMessage = new HttpRequestMessage(
            HttpMethod.Patch,
            BuildAcademyPath($"user/enrollments/users/{userId}/limit-access"));

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        var response = await _httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Academy limit-access failed for user {UserId}. StatusCode: {StatusCode}",
                userId,
                response.StatusCode);

            throw new HttpRequestException(
                $"Academy limit-access failed: {response.StatusCode}",
                null,
                response.StatusCode);
        }
    }

    public async Task<IEnumerable<DroneResponseDto>> GetDronesBulk(IEnumerable<Guid> droneIds)
    {
        if (droneIds == null || !droneIds.Any())
            return Enumerable.Empty<DroneResponseDto>();

        var distinctIds = droneIds.Where(id => id != Guid.Empty).Distinct().ToList();

        if (distinctIds.Count == 0)
            return Enumerable.Empty<DroneResponseDto>();

        var droneDict = new Dictionary<Guid, DroneResponseDto>();
        var missingIds = new List<Guid>();

        var cacheReadTasks = distinctIds.Select(async id =>
        {
            var cacheKey = $"drone:{id}";
            var cacheValue = await _distributedCache.GetStringAsync(cacheKey);
            _logger.LogInformation($"Drone with id [{id}] found in cache.");
            return (Id: id, CacheValue: cacheValue);
        });

        var cachedDrones = await Task.WhenAll(cacheReadTasks);

        foreach (var (id, cacheValue) in cachedDrones)
        {
            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                missingIds.Add(id);
                continue;
            }

            try
            {
                var cachedDrone = JsonSerializer.Deserialize<DroneResponseDto>(cacheValue);
                if (cachedDrone == null)
                {
                    missingIds.Add(id);
                    continue;
                }

                droneDict[id] = cachedDrone;
            }
            catch
            {
                missingIds.Add(id);
            }
        }

        if (missingIds.Any())
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    BuildAcademyPath($"drones/bulk"),
                    missingIds
                );

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogError("Academy service unavailable (bulk request).");
                        throw new HttpRequestException(
                            "Academy service unavailable",
                            null,
                            System.Net.HttpStatusCode.ServiceUnavailable);
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException(
                            "Bad request when calling Academy bulk API",
                            null,
                            System.Net.HttpStatusCode.BadRequest);
                    }

                    throw new HttpRequestException(
                        $"Academy bulk API error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }

                var dronesFromApi = await response.Content.ReadFromJsonAsync<IEnumerable<DroneResponseDto>>(_jsonOptions);

                if (dronesFromApi == null || !dronesFromApi.Any())
                {
                    _logger.LogWarning("No drones returned from Academy API.");
                    return distinctIds.Where(id => droneDict.ContainsKey(id)).Select(id => droneDict[id]);
                }

                var cacheWriteTasks = new List<Task>();

                foreach (var drone in dronesFromApi)
                {
                    droneDict[drone.DroneID] = drone;
                    string droneKeyToWrite = $"drone:{drone.DroneID}";
                    string droneCacheString = JsonSerializer.Serialize(drone);
                    cacheWriteTasks.Add(_distributedCache.SetStringAsync(droneKeyToWrite, droneCacheString, DroneCacheOptions));
                }

                if (cacheWriteTasks.Count > 0)
                    await Task.WhenAll(cacheWriteTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Academy bulk API");
                throw;
            }
        }

        return distinctIds
            .Where(id => droneDict.ContainsKey(id))
            .Select(id => droneDict[id]);

    }

    public async Task<DroneResponseDto> GetDroneDetailById(Guid droneId)
    {
        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync(
            BuildAcademyPath($"drones/{droneId}"));
        if (!httpResponseMsg.IsSuccessStatusCode)
        {
            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Dịch vụ Academy tạm thời không khả dụng.");
                return null;
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Không tìm thấy drone trong Academy Microservice.");
                return null;
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                throw new HttpRequestException($"Lỗi Academy service: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
            }
        }
        DroneResponseDto? drone = (await httpResponseMsg.Content.ReadFromJsonAsync<DroneResponseDto>(_jsonOptions));
        if (drone == null)
        {
            throw new NotFoundException("NOT FOUND DRONE");
        }
        return drone;
    }

    public async Task<CodeResponse> GenerateAssignCodesAsync(GenerateCodesRequestDTO request, string email)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        try
        {
            // Get user email and name from current user context
            //var email = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Không thể lấy email của người dùng hiện tại.");

            var fullName = _currentUserService.UserName ?? "User";

            // Build the correct request DTO for generate-assign endpoint
            var generateWithAssignRequest = new GenerateWithAssignCodeRequestDTO
            {
                ClubId = request.ClubId,
                CourseId = request.CourseId,
                Quantity = request.Quantity,
                Email = email,
                FullName = fullName
            };

            // Get dynamic service token
            //var serviceToken = await GetValidServiceTokenAsync();
            //_logger.LogInformation("serviceToken: {serviceToken}", serviceToken);

            //// Service-to-service call with dynamic token
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, BuildAcademyPath("codes/generate-assign"))
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(generateWithAssignRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

            //// Add dynamic service token
            //requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", serviceToken);
            const string ServiceToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI4YTY0ZDk1ZS1mMDQxLTQ5ZjctYmMxOC1hODJhZWNkODE2MTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJqdGkiOiJhMjNlZDg2Ni1jZGU1LTQxNjctODRkOC04M2Q5MzU1OWU5OGQiLCJleHAiOjE3Nzg0MTU3MjgsImlzcyI6IkRyb25pdmVyc2UuSWRlbnRpdHkiLCJhdWQiOiJEcm9uaXZlcnNlLklkZW50aXR5In0.f-8scgOBuJ7xZQn5vtkAI4kZXaezNCTiUBNYbF1L4zQ";
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceToken);

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy dữ liệu khi gọi Academy API codes/generate-assign.");
                    throw new KeyNotFoundException("Không tìm thấy dữ liệu để gán code.");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
                throw new HttpRequestException(
                    $"Academy service lỗi khi gọi codes/generate-assign: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }
            var result = await response.Content.ReadFromJsonAsync<CodeResponse>(_jsonOptions);
            if (result == null)
                throw new InvalidOperationException("Không nhận được phản hồi hợp lệ từ Academy service khi gán code.");
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Lỗi khi gọi Academy API codes/generate-assign.");
            throw;
        }
    }

    public async Task<CreateCodesResponse> GenerateCodes(GenerateCodesRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("codes/generate"),
                request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy dữ liệu khi gọi Academy API codes/generate.");
                    throw new KeyNotFoundException("Không tìm thấy dữ liệu để tạo code.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);

                throw new HttpRequestException(
                    $"Academy service lỗi khi gọi codes/generate: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var created = await response.Content.ReadFromJsonAsync<CreateCodesResponse>(_jsonOptions);
            if (created == null)
                throw new InvalidOperationException("Không nhận được phản hồi tạo code hợp lệ từ Academy service.");

            return created;
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Lỗi khi gọi Academy API codes/generate cho ClubId {ClubId}, CourseId {CourseId}", request.ClubId, request.CourseId);
            throw;
        }
    }

    public async Task<IEnumerable<SimpleLabResponse>> GetLabsByIds(IEnumerable<Guid> labIds)
    {
        var ids = labIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        var labById = new Dictionary<Guid, SimpleLabResponse>();
        var missingIds = new List<Guid>();

        foreach (var id in ids)
        {
            var cacheKey = BuildLabCacheKey(id);
            var cacheValue = await _distributedCache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                missingIds.Add(id);
                continue;
            }

            try
            {
                var cachedLab = JsonSerializer.Deserialize<SimpleLabResponse>(cacheValue, _jsonOptions);
                if (cachedLab != null)
                {
                    labById[id] = cachedLab;
                    continue;
                }
            }
            catch
            {
                // ignore invalid cache and fallback to API
            }

            missingIds.Add(id);
        }

        if (missingIds.Count > 0)
        {
            var labsFromApi = await GetLabsBulk(missingIds);

            foreach (var lab in labsFromApi)
            {
                labById[lab.LabID] = lab;

                var cacheKey = BuildLabCacheKey(lab.LabID);
                var cacheString = JsonSerializer.Serialize(lab);
                await _distributedCache.SetStringAsync(cacheKey, cacheString, LabCacheOptions);
            }
        }

        return ids
            .Where(labById.ContainsKey)
            .Select(id => labById[id])
            .ToList();
    }

    private async Task<IEnumerable<SimpleLabResponse>> GetLabsBulk(IEnumerable<Guid> labIds)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("labs/bulk"),
                labIds);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy lab trong Academy Microservice khi gọi API bulk.");
                    return Array.Empty<SimpleLabResponse>();
                }

                _logger.LogWarning("Academy service lỗi khi gọi labs/bulk: {StatusCode}", response.StatusCode);
                return Array.Empty<SimpleLabResponse>();
            }

            var payload = await response.Content.ReadAsStringAsync();

            // Deserialize trực tiếp thẳng vào IEnumerable<SimpleLabResponse>
            var labs = JsonSerializer.Deserialize<IEnumerable<SimpleLabResponse>>(payload, _jsonOptions);

            if (labs == null)
                return [];

            return labs
                .Where(x => x != null && x.LabID != Guid.Empty)
                .Select(x => new SimpleLabResponse
                {
                    LabID = x.LabID,
                    LabNameVN = x.LabNameVN,
                    LabNameEN = x.LabNameEN
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Academy API labs/bulk.");
            return Array.Empty<SimpleLabResponse>();
        }
    }

    public async Task<FeedbackResponseDto> GetFeedbackById(Guid feedbackId)
    {
        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync(
            BuildAcademyPath($"feedbacks/{feedbackId}"));

        if (!httpResponseMsg.IsSuccessStatusCode)
        {
            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Feedback with ID {FeedbackId} not found in Academy Microservice.", feedbackId);
                return null;
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                //fallback data
                throw new HttpRequestException($"Academy service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
            }
        }
        FeedbackResponseDto? feedback = await httpResponseMsg.Content.ReadFromJsonAsync<FeedbackResponseDto>();
        if (feedback == null)
        {
            throw new ArgumentException("Invalid feedbackID");
        }
        return feedback;
    }

    public async Task<bool> IsLabExist(Guid labId)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                BuildAcademyPath($"labs/{labId}/exist"));

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking lab existence with ID {LabId}", labId);
            return false;
        }
    }

    public async Task<CertificateDetailResponse?> GetCertificateById(Guid certificateId)
    {
        try
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync(
                BuildAcademyPath($"certificates/{certificateId}"));

            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Certificate with ID {CertificateId} not found in Academy Microservice.", certificateId);
                return null;
            }

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                _logger.LogWarning("Academy service error when getting certificate {CertificateId}: {StatusCode}", certificateId, httpResponseMsg.StatusCode);
                return null;
            }

            var certificate = await httpResponseMsg.Content.ReadFromJsonAsync<CertificateDetailResponse>();
            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching certificate with ID {CertificateId} from Academy service.", certificateId);
            return null;
        }
    }

    public async Task<IEnumerable<SimpleCertificateResponse>> GetCertificatesBulk(
      IEnumerable<Guid> certificateIds,
      CancellationToken cancellationToken = default)
    {
        if (certificateIds == null)
            return [];

        var distinctIds = certificateIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("certificates/bulk"),
                distinctIds,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Academy bulk API failed. StatusCode: {StatusCode}, Count: {Count}",
                    response.StatusCode,
                    distinctIds.Count);

                return [];
            }

            var certificates = await response.Content.ReadFromJsonAsync<
                IEnumerable<SimpleCertificateResponse>>(cancellationToken);

            return certificates ?? [];
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Academy bulk API timeout. Count: {Count}", distinctIds.Count);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calling Academy certificates bulk API. Count: {Count}",
                distinctIds.Count);

            return [];
        }
    }

    public async Task<IEnumerable<SimpleLevelResponse>> GetLevelsBulk(
      IEnumerable<Guid> levelIds,
      CancellationToken cancellationToken = default)
    {
        if (levelIds == null)
            return [];

        var distinctIds = levelIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("levels/bulk"),
                distinctIds,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Academy levels bulk API failed. StatusCode: {StatusCode}, Count: {Count}",
                    response.StatusCode,
                    distinctIds.Count);

                return [];
            }

            var levels = await response.Content.ReadFromJsonAsync<
                IEnumerable<SimpleLevelResponse>>(cancellationToken);

            return levels ?? [];
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Academy levels bulk API timeout. Count: {Count}", distinctIds.Count);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calling Academy levels bulk API. Count: {Count}",
                distinctIds.Count);

            return [];
        }
    }

    public async Task<PagedCourseBulkResponse> GetCourseById(
        Guid clubId,
        CourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new CourseBulkSearchRequest();

        if (clubId == Guid.Empty)
            return CreatePagedCourseResponse([], 0);

        string queryString = BuildCourseBulkSearchQuery(searchRequest);
        var pageCacheKey = BuildBulkCoursesPageCacheKey(clubId, queryString);

        var cachedValue = await _distributedCache.GetStringAsync(pageCacheKey);
        if (!string.IsNullOrWhiteSpace(cachedValue))
        {
            try
            {
                var cachedResponse = JsonSerializer.Deserialize<PagedCourseBulkResponse>(cachedValue, _jsonOptions);
                if (cachedResponse?.Items != null)
                    return cachedResponse;
            }
            catch
            {
                // ignore invalid cache and fallback to API
            }
        }


        HttpResponseMessage response = await _httpClient.GetAsync(
            $"{BuildAcademyPath($"courses/club/{clubId}")}?{queryString}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Dịch vụ Academy tạm thời không khả dụng.");
                return CreatePagedCourseResponse([], 0);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Không tìm thấy khóa học trong Academy Microservice.");
                return CreatePagedCourseResponse([], 0);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                throw new HttpRequestException(
                    $"Lỗi Academy service: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }
        }

        var payload = await response.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);
        var result = payload ?? CreatePagedCourseResponse([], 0);

        var cacheString = JsonSerializer.Serialize(result);
        await _distributedCache.SetStringAsync(pageCacheKey, cacheString, CourseCacheOptions);

        return result;
    }

    public async Task<IEnumerable<SimpleCourseResponse>> GetCoursesByIdsSimple(
        Guid clubId,
        CancellationToken cancellationToken = default)
    {
        if (clubId == Guid.Empty)
            return [];

        try
        {
            var response = await _httpClient.GetAsync(
                BuildAcademyPath($"courses/club/{clubId}/simple"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy khóa học khi gọi courses/club/{ClubId}/simple.", clubId);
                    return [];
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
                }

                _logger.LogWarning("Academy service lỗi khi gọi courses/club/{ClubId}/simple: {StatusCode}", clubId, response.StatusCode);
                return [];
            }

            var wrapped = await response.Content.ReadFromJsonAsync<SuccessResponse<IEnumerable<SimpleCourseResponse>>>(_jsonOptions, cancellationToken);
            return wrapped?.Data?
                .Where(x => x != null && x.CourseId != Guid.Empty)
                .ToList()
                ?? [];
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout khi gọi courses/club/{ClubId}/simple", clubId);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi courses/club/{ClubId}/simple", clubId);
            return [];
        }
    }

    /// <summary>
    /// Lấy danh sách khóa học HOT theo danh sách ID (có phân trang) từ Academy Microservice.
    /// Dữ liệu được cache theo tập ID + tham số phân trang để giảm số lần gọi mạng.
    /// </summary>
    /// <param name="clubId">ID câu lạc bộ cần lấy khóa học.</param>
    /// <param name="searchRequest">Thông tin phân trang cho dữ liệu khóa học hot.</param>
    /// <returns>Kết quả danh sách khóa học hot theo trang hiện tại.</returns>
    public async Task<PagedCourseBulkResponse> GetHotCoursesByIds(
        Guid clubId,
        HotCoursesSearchRequest searchRequest)
    {
        searchRequest ??= new HotCoursesSearchRequest();

        if (clubId == Guid.Empty)
            return CreatePagedCourseResponse([], 0);

        var queryString = BuildHotCoursesSearchQuery(searchRequest);
        var cacheKey = BuildHotBulkCoursesCacheKey(clubId, queryString);

        var cachedValue = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cachedValue))
        {
            try
            {
                var cachedResponse = JsonSerializer.Deserialize<PagedCourseBulkResponse>(cachedValue, _jsonOptions);
                if (cachedResponse?.Items != null)
                    return cachedResponse;
            }
            catch
            {
                // Bỏ qua cache lỗi định dạng và gọi API thật
            }
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"{BuildAcademyPath($"courses/club/{clubId}/hot")}?{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Gọi Academy API lấy khóa học hot thất bại. StatusCode: {StatusCode}, Số lượng CourseId: {Count}",
                    response.StatusCode,
                    0);

                return CreatePagedCourseResponse([], 0);
            }

            var payload = await response.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);
            var hotCourses = payload?.Items?
                .Where(x => x != null && x.CourseId != Guid.Empty)
                .ToList() ?? [];

            var result = CreatePagedCourseResponse(hotCourses, payload?.TotalItems ?? hotCourses.Count);

            var cacheString = JsonSerializer.Serialize(result);
            await _distributedCache.SetStringAsync(cacheKey, cacheString, HotCourseCacheOptions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Lỗi khi gọi Academy API lấy danh sách khóa học hot. Số lượng CourseId: {Count}",
                0);

            return CreatePagedCourseResponse([], 0);
        }
    }

    public async Task<PagedCourseBulkResponse> GetCoursesByIdsManagement(
    Guid clubId,
    ManagerCourseBulkSearchRequest searchRequest,
    CancellationToken cancellationToken = default)
    {
        searchRequest ??= new ManagerCourseBulkSearchRequest();

        if (clubId == Guid.Empty)
            return CreatePagedCourseResponse([], 0);

        var queryString = BuildManagerCourseBulkSearchQuery(searchRequest);

        try
        {
            var response = await _httpClient.GetAsync(
                $"{BuildAcademyPath($"courses/club/{clubId}/management")}?{queryString}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service không khả dụng.");
                    return CreatePagedCourseResponse([], 0);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy course.");
                    return CreatePagedCourseResponse([], 0);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Request không hợp lệ", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Academy service error: {response.StatusCode}",
                        null,
                        response.StatusCode);
                }
            }

            var result = await response.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);

            return result ?? CreatePagedCourseResponse([], 0);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout khi gọi courses/by-ids/management");
            return CreatePagedCourseResponse([], 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi courses/by-ids/management");
            return CreatePagedCourseResponse([], 0);
        }
    }

    public async Task<SimpleVRSimulatorResponse> GetSimpleVRSimulator(Guid vrSimulatorId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                BuildAcademyPath($"vr-simulators/{vrSimulatorId}/check"));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy VR Simulator với ID {VrSimulatorId}", vrSimulatorId);

                    throw new NotFoundException("Không tìm thấy VR Simulator");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException(
                        "Yêu cầu không hợp lệ khi gọi Academy service.",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service không khả dụng.");
                    throw new HttpRequestException(
                        "Dịch vụ Academy tạm thời không khả dụng.",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }

                throw new HttpRequestException(
                    $"Lỗi Academy service: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<SimpleVRSimulatorResponse>(_jsonOptions);

            if (result == null)
            {
                throw new NotFoundException("Không tìm thấy dữ liệu VR Simulator hợp lệ");
            }

            return result;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API VR Simulator với ID {VrSimulatorId}", vrSimulatorId);
            throw;
        }
    }

    public async Task<IEnumerable<SimpleVRSimulatorResponse>> GetVRSimulatorsByIds(IEnumerable<Guid> vrSimulatorIds)
    {
        if (vrSimulatorIds == null || !vrSimulatorIds.Any())
            return [];

        var distinctIds = vrSimulatorIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("vr-simulators/check"),
                distinctIds
            );

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy VR Simulator theo danh sách ID.");
                    throw new NotFoundException("Không tìm thấy VR Simulator");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException(
                        "Yêu cầu không hợp lệ khi gọi Academy service.",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service không khả dụng.");
                    throw new HttpRequestException(
                        "Dịch vụ Academy tạm thời không khả dụng.",
                        null,
                        System.Net.HttpStatusCode.ServiceUnavailable);
                }

                throw new HttpRequestException(
                    $"Lỗi Academy service: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content
                .ReadFromJsonAsync<IEnumerable<SimpleVRSimulatorResponse>>(_jsonOptions);

            if (result == null || !result.Any())
            {
                throw new NotFoundException("Không tìm thấy dữ liệu VR Simulator");
            }

            return result
                .Where(x => x != null && x.VRSimulatorId != Guid.Empty)
                .ToList();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API VR Simulator bulk");
            throw;
        }
    }

    public async Task<IEnumerable<Guid>> GetUserLevelIds(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                BuildAcademyPath($"user/levels/{userId}/ids"));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy level cho user {UserId}", userId);
                    return Enumerable.Empty<Guid>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException(
                        "Yêu cầu không hợp lệ khi gọi API user level ids",
                        null,
                        System.Net.HttpStatusCode.BadRequest);
                }

                _logger.LogError("Lỗi khi gọi API user level ids: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<Guid>();
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<Guid>>(_jsonOptions);

            return result ?? Enumerable.Empty<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user level ids for user {UserId}", userId);
            return Enumerable.Empty<Guid>();
        }
    }

    public async Task<CodeStatsOverviewResponse> GetCodeStatsByClub(Guid clubId)
    {
        try
        {
            var response = await _httpClient.GetAsync(BuildAcademyPath($"codes/clubs/{clubId}/stats"));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy code stats cho club {ClubId}", clubId);
                    return new CodeStatsOverviewResponse();
                }

                _logger.LogWarning("Academy service lỗi khi gọi codes/clubs/{ClubId}/stats: {StatusCode}", clubId, response.StatusCode);
                return new CodeStatsOverviewResponse();
            }

            var codeStats = await response.Content.ReadFromJsonAsync<CodeStatsOverviewResponse>(_jsonOptions);
            return codeStats ?? new CodeStatsOverviewResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Academy API codes/clubs/{ClubId}/stats", clubId);
            return new CodeStatsOverviewResponse();
        }
    }

    public async Task<CodeStatsOverviewResponse> GetCodeStatsAdmin()
    {
        try
        {
            var response = await _httpClient.GetAsync(BuildAcademyPath("codes/admin/stats"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Academy service lỗi khi gọi codes/admin/stats: {StatusCode}", response.StatusCode);
                return new CodeStatsOverviewResponse();
            }

            var codeStats = await response.Content.ReadFromJsonAsync<CodeStatsOverviewResponse>(_jsonOptions);
            return codeStats ?? new CodeStatsOverviewResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi Academy API codes/admin/stats");
            return new CodeStatsOverviewResponse();
        }
    }

    private static string BuildManagerCourseBulkSearchQuery(ManagerCourseBulkSearchRequest request)
    {
        var queryParts = new List<string>
    {
        $"CurrentPage={request.CurrentPage}",
        $"PageSize={request.PageSize}"
    };

        // Level
        //if (request.Level.HasValue)
        //{
        //    queryParts.Add($"Level={(int)request.Level.Value}");
        //}

        // ProfitType
        if (request.ProfitType.HasValue)
        {
            queryParts.Add($"ProfitType={(int)request.ProfitType.Value}");
        }

        if (request.LevelId.HasValue && request.LevelId.Value != Guid.Empty)
        {
            queryParts.Add($"LevelId={request.LevelId.Value}");
        }

        // SortBy (có default nên luôn gửi)
        queryParts.Add($"CourseSortBy={(int)request.CourseSortBy!.Value}");

        // SortDirection (có default nên luôn gửi)
        queryParts.Add($"CourseSortDirection={(int)request.CourseSortDirection!.Value}");

        return string.Join("&", queryParts);
    }

    private static string BuildBulkCoursesPageCacheKey(Guid clubId, string queryString)
    {
        var raw = $"{clubId}|{queryString}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hash = Convert.ToHexString(hashBytes);

        return $"course:bulk:page:{hash}";
    }

    private static string BuildHotBulkCoursesCacheKey(Guid clubId, string queryString)
    {
        var raw = $"{clubId}|{queryString}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hash = Convert.ToHexString(hashBytes);

        return $"course:bulk:hot:{hash}";
    }

    private static string BuildLabCacheKey(Guid labId)
    {
        return $"lab:simple:{labId}";
    }

    private static string BuildCourseBulkSearchQuery(CourseBulkSearchRequest searchRequest)
    {
        var queryParts = new List<string>
        {
            $"CurrentPage={searchRequest.CurrentPage}",
            $"PageSize={searchRequest.PageSize}"
        };

        //if (searchRequest.Level.HasValue)
        //{
        //    queryParts.Add($"Level={(int)searchRequest.Level.Value}");
        //}

        if (searchRequest.ParticipationSort.HasValue)
        {
            queryParts.Add($"ParticipationSort={(int)searchRequest.ParticipationSort.Value}");
        }

        if (searchRequest.LevelId.HasValue && searchRequest.LevelId.Value != Guid.Empty)
        {
            queryParts.Add($"LevelId={searchRequest.LevelId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(searchRequest.CourseName))
        {
            queryParts.Add($"CourseName={Uri.EscapeDataString(searchRequest.CourseName.Trim())}");
        }

        return string.Join("&", queryParts);
    }

    private static string BuildHotCoursesSearchQuery(HotCoursesSearchRequest searchRequest)
    {
        var queryParts = new List<string>
        {
            $"CurrentPage={searchRequest.CurrentPage}",
            $"PageSize={searchRequest.PageSize}"
        };

        if (searchRequest.LevelId.HasValue && searchRequest.LevelId.Value != Guid.Empty)
        {
            queryParts.Add($"LevelId={searchRequest.LevelId.Value}");
        }

        return string.Join("&", queryParts);
    }

    private static PagedCourseBulkResponse CreatePagedCourseResponse(IEnumerable<CourseBulkResponseDTO> items, int totalItems)
    {
        return new PagedCourseBulkResponse
        {
            TotalItems = totalItems < 0 ? 0 : totalItems,
            Items = items
        };
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

    private async Task<SimpleLabResponse?> GetLabById(Guid labId)
    {
        try
        {
            var httpResponseMsg = await _httpClient.GetAsync(BuildAcademyPath($"labs/{labId}"));

            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Lab with ID {LabId} not found in Academy Microservice.", labId);
                return null;
            }

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                _logger.LogWarning("Academy service error when getting lab {LabId}: {StatusCode}", labId, httpResponseMsg.StatusCode);
                return null;
            }

            var payload = await httpResponseMsg.Content.ReadAsStringAsync();
            var wrapped = JsonSerializer.Deserialize<SuccessResponse<AcademyLabDetailDto>>(payload, _jsonOptions);
            var labData = wrapped?.Data?.Lab;

            if (labData == null)
            {
                labData = JsonSerializer.Deserialize<AcademyLabDto>(payload, _jsonOptions);
            }

            if (labData == null)
                return null;

            return new SimpleLabResponse
            {
                LabID = labData.LabID,
                LabNameVN = labData.NameVN,
                LabNameEN = labData.NameEN
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lab with ID {LabId} from Academy service.", labId);
            return null;
        }
    }

    /// <summary>
    /// Get valid service token, with auto-refresh when expired
    /// </summary>
    private async Task<string> GetValidServiceTokenAsync()
    {
        try
        {
            // Return cached token if still valid
            if (!string.IsNullOrEmpty(_cachedServiceToken) && _clock.Now < _tokenExpiresAt)
            {
                _logger.LogInformation("Using cached service token");
                return _cachedServiceToken;
            }

            // Get new token from Identity service
            var serviceId = "community-service";
            var apiKey = _configuration["SERVICE_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("SERVICE_API_KEY not configured");
            }

            _logger.LogInformation("Requesting new service token from Identity service");
            var token = await _identityMicroserviceClient.GetServiceTokenAsync(serviceId, apiKey);

            // Cache token with 50-minute expiration (service token TTL is 1 hour)
            _cachedServiceToken = token;
            _tokenExpiresAt = _clock.Now.AddMinutes(50);

            _logger.LogInformation("Service token obtained and cached");
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining service token");
            throw;
        }
    }
}



internal class AcademyLabDetailDto
{
    public AcademyLabDto? Lab { get; set; }
}

internal class AcademyLabDto
{
    public Guid LabID { get; set; }
    public string NameVN { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
}

public class CertificateDetailResponse
{
    public Guid CertificateID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string CertificateName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string LogoCertificate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
    public Guid CreateBy { get; set; }
    public Guid UpdateBy { get; set; }
    public DateTime UpdateAt { get; set; }
}

public class BulkCodeAssignmentResponse
{
    public int TotalAssigned { get; set; }
    public List<CodeAssignmentResponse> AssignedItems { get; set; } = [];
}

public class CodeAssignmentResponse
{
    public string CodeId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class CodeResponse
{
    public string CodeID { get; set; }
    public string CourseID { get; set; }
    public string ClubID { get; set; }
    public Guid? OwnedUserID { get; set; }
    public Guid? UsedByUserID { get; set; }
    public DateTime? UsedDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public CodeStatusEnum Status { get; set; }
}

public record GenerateCodesRequestDTO
{
    [Required(ErrorMessage = "Không thể thiếu mã câu lạc bộ")]
    public required Guid ClubId { get; set; }
    [Required(ErrorMessage = "Không thể thiếu mã khóa học")]
    public required Guid CourseId { get; set; }
    [Range(1, 50, ErrorMessage = "Tạo từ 1 tới 50 mã")]
    public int Quantity { get; set; }
}