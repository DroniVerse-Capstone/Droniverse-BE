using Droniverse.Academy.Application.Enums;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Community.Application.HttpClients;

// DTO for Academy microservice response
public class CourseResponse
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Level { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class AcademyMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AcademyMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache; //Redis Cache
    private readonly IHostEnvironment _environment;
    private readonly ICurrentUserService _currentUserService;
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
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public AcademyMicroserviceClient(
        HttpClient httpClient,
        ILogger<AcademyMicroserviceClient> logger,
        IDistributedCache distributedCache,
        IHostEnvironment environment,
        ICurrentUserService currentUserService
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _environment = environment;
        _currentUserService = currentUserService;
    }

    public async Task<CodeResponse> GenerateAssignCodesAsync(GenerateCodesRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        try
        {
            // Get user email and name from current user context
            var email = _currentUserService.Email;
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

            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("codes/generate-assign"),
                generateWithAssignRequest);

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

    public async Task<PagedCourseBulkResponse> GetCourseById(
        IEnumerable<Guid> courseIds,
        CourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new CourseBulkSearchRequest();

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        string queryString = BuildCourseBulkSearchQuery(searchRequest);
        var pageCacheKey = BuildBulkCoursesPageCacheKey(ids, queryString);

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

        // ALL/NotOwned: luôn gọi trực tiếp Academy để service bên kia tự lọc theo query + danh sách ids truyền vào
        if (searchRequest.CourseOwner is CourseOwnerFilter.All or CourseOwnerFilter.NotOwned)
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.PostAsJsonAsync(
                $"{BuildAcademyPath("courses/by-ids")}?{queryString}",
                new GetCoursesByIdsRequestDTO { CourseIds = ids });

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Dịch vụ Academy tạm thời không khả dụng.");
                    return CreatePagedCourseResponse([], 0);
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy khóa học trong Academy Microservice.");
                    return CreatePagedCourseResponse([], 0);
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Lỗi Academy service: {httpResponseMsg.StatusCode}",
                        null,
                        httpResponseMsg.StatusCode);
                }
            }

            var pagedCourses = await httpResponseMsg.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);
            var result = pagedCourses ?? CreatePagedCourseResponse([], 0);

            var cacheString = JsonSerializer.Serialize(result);
            await _distributedCache.SetStringAsync(pageCacheKey, cacheString, CourseCacheOptions);

            return result;
        }

        // Owned nhưng không có course nào thuộc club
        if (ids.Count == 0)
            return CreatePagedCourseResponse([], 0);

        HttpResponseMessage ownedHttpResponseMsg = await _httpClient.PostAsJsonAsync(
            $"{BuildAcademyPath("courses/by-ids")}?{queryString}",
            new GetCoursesByIdsRequestDTO { CourseIds = ids });

        if (!ownedHttpResponseMsg.IsSuccessStatusCode)
        {
            if (ownedHttpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogError("Dịch vụ Academy tạm thời không khả dụng.");
                return CreatePagedCourseResponse([], 0);
            }
            else if (ownedHttpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Không tìm thấy khóa học trong Academy Microservice.");
                return CreatePagedCourseResponse([], 0);
            }
            else if (ownedHttpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                throw new HttpRequestException(
                    $"Lỗi Academy service: {ownedHttpResponseMsg.StatusCode}",
                    null,
                    ownedHttpResponseMsg.StatusCode);
            }
        }

        var ownedPagedCourses = await ownedHttpResponseMsg.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);
        var ownedResult = ownedPagedCourses ?? CreatePagedCourseResponse([], 0);

        var ownedCacheString = JsonSerializer.Serialize(ownedResult);
        await _distributedCache.SetStringAsync(pageCacheKey, ownedCacheString, CourseCacheOptions);

        return ownedResult;
    }

    public async Task<IEnumerable<SimpleCourseResponse>> GetCoursesByIdsSimple(
        IEnumerable<Guid> courseIds,
        CancellationToken cancellationToken = default)
    {
        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                BuildAcademyPath("courses/by-ids/simple"),
                new GetCoursesByIdsRequestDTO { CourseIds = ids },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Không tìm thấy khóa học khi gọi courses/by-ids/simple.");
                    return [];
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Yêu cầu không hợp lệ khi gọi Academy service.", null, System.Net.HttpStatusCode.BadRequest);
                }

                _logger.LogWarning("Academy service lỗi khi gọi courses/by-ids/simple: {StatusCode}", response.StatusCode);
                return [];
            }

            var data = await response.Content.ReadFromJsonAsync<IEnumerable<SimpleCourseResponse>>(_jsonOptions, cancellationToken);
            return data?
                .Where(x => x != null && x.CourseId != Guid.Empty)
                .ToList()
                ?? [];
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout khi gọi courses/by-ids/simple");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi courses/by-ids/simple");
            return [];
        }
    }

    /// <summary>
    /// Lấy danh sách khóa học HOT theo danh sách ID (có phân trang) từ Academy Microservice.
    /// Dữ liệu được cache theo tập ID + tham số phân trang để giảm số lần gọi mạng.
    /// </summary>
    /// <param name="courseIds">Danh sách ID khóa học cần lấy.</param>
    /// <param name="searchRequest">Thông tin phân trang cho dữ liệu khóa học hot.</param>
    /// <returns>Kết quả danh sách khóa học hot theo trang hiện tại.</returns>
    public async Task<PagedCourseBulkResponse> GetHotCoursesByIds(
        IEnumerable<Guid> courseIds,
        HotCoursesSearchRequest searchRequest)
    {
        searchRequest ??= new HotCoursesSearchRequest();

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return CreatePagedCourseResponse([], 0);

        var queryString = BuildHotCoursesSearchQuery(searchRequest);
        var cacheKey = BuildHotBulkCoursesCacheKey(ids, queryString);

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
            var response = await _httpClient.PostAsJsonAsync(
                $"{BuildAcademyPath("courses/by-ids/hot")}?{queryString}",
                new GetCoursesByIdsRequestDTO { CourseIds = ids });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Gọi Academy API lấy khóa học hot thất bại. StatusCode: {StatusCode}, Số lượng CourseId: {Count}",
                    response.StatusCode,
                    ids.Count);

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
                ids.Count);

            return CreatePagedCourseResponse([], 0);
        }
    }

    public async Task<PagedCourseBulkResponse> GetCoursesByIdsManagement(
    IEnumerable<Guid> courseIds,
    ManagerCourseBulkSearchRequest searchRequest,
    CancellationToken cancellationToken = default)
    {
        searchRequest ??= new ManagerCourseBulkSearchRequest();

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (!ids.Any())
            return CreatePagedCourseResponse([], 0);

        var queryString = BuildManagerCourseBulkSearchQuery(searchRequest);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BuildAcademyPath("courses/by-ids/management")}?{queryString}",
                new GetCoursesByIdsRequestDTO { CourseIds = ids },
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

    private static string BuildManagerCourseBulkSearchQuery(ManagerCourseBulkSearchRequest request)
    {
        var queryParts = new List<string>
    {
        $"CurrentPage={request.CurrentPage}",
        $"PageSize={request.PageSize}"
    };

        // Level
        if (request.Level.HasValue)
        {
            queryParts.Add($"Level={(int)request.Level.Value}");
        }

        // ProfitType
        if (request.ProfitType.HasValue)
        {
            queryParts.Add($"ProfitType={(int)request.ProfitType.Value}");
        }

        // SortBy (có default nên luôn gửi)
        queryParts.Add($"CourseSortBy={(int)request.CourseSortBy!.Value}");

        // SortDirection (có default nên luôn gửi)
        queryParts.Add($"CourseSortDirection={(int)request.CourseSortDirection!.Value}");

        return string.Join("&", queryParts);
    }

    private static string BuildBulkCoursesPageCacheKey(IEnumerable<Guid> courseIds, string queryString)
    {
        var sortedIds = courseIds
            .OrderBy(x => x)
            .Select(x => x.ToString())
            .ToList();

        var raw = $"{string.Join(',', sortedIds)}|{queryString}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hash = Convert.ToHexString(hashBytes);

        return $"course:bulk:page:{hash}";
    }

    private static string BuildHotBulkCoursesCacheKey(IEnumerable<Guid> courseIds, string queryString)
    {
        var sortedIds = courseIds
            .OrderBy(x => x)
            .Select(x => x.ToString())
            .ToList();

        var raw = $"{string.Join(',', sortedIds)}|{queryString}";
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
            $"PageSize={searchRequest.PageSize}",
            $"CourseOwner={(int)searchRequest.CourseOwner}"
        };

        if (searchRequest.Level.HasValue)
        {
            queryParts.Add($"Level={(int)searchRequest.Level.Value}");
        }

        if (searchRequest.ParticipationSort.HasValue)
        {
            queryParts.Add($"ParticipationSort={(int)searchRequest.ParticipationSort.Value}");
        }

        if (!string.IsNullOrWhiteSpace(searchRequest.CourseName))
        {
            queryParts.Add($"CourseName={Uri.EscapeDataString(searchRequest.CourseName.Trim())}");
        }

        return string.Join("&", queryParts);
    }

    private static string BuildHotCoursesSearchQuery(HotCoursesSearchRequest searchRequest)
    {
        return $"CurrentPage={searchRequest.CurrentPage}&PageSize={searchRequest.PageSize}";
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