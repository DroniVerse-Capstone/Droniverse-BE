using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        IHostEnvironment environment
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        _environment = environment;
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
                HttpMethod.Head,
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

        if (ids.Count == 0)
            return CreatePagedCourseResponse([], 0);

        string queryString = BuildCourseBulkSearchQuery(searchRequest);

        var courseById = new Dictionary<Guid, CourseBulkResponseDTO>();
        var missingIds = new List<Guid>();

        // 1) Read cache first
        foreach (var id in ids)
        {
            var cacheKey = BuildBulkCourseCacheKey(id, queryString);
            var cacheValue = await _distributedCache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                missingIds.Add(id);
                continue;
            }

            try
            {
                var cachedCourse = JsonSerializer.Deserialize<CourseBulkResponseDTO>(cacheValue, _jsonOptions);
                if (cachedCourse != null)
                {
                    courseById[id] = cachedCourse;
                    continue;
                }
            }
            catch
            {
                // ignore invalid cache and fallback to API
            }

            missingIds.Add(id);
        }

        // 2) Call API only for missing ids
        if (missingIds.Count > 0)
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.PostAsJsonAsync(
                $"{BuildAcademyPath("courses/by-ids")}?{queryString}",
                new GetCoursesByIdsRequestDTO { CourseIds = missingIds });

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service unavailable.");
                    var fallbackItems = ids.Where(courseById.ContainsKey).Select(id => courseById[id]).ToList();
                    return CreatePagedCourseResponse(fallbackItems, fallbackItems.Count);
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Courses not found in Academy Microservice.");
                    var fallbackItems = ids.Where(courseById.ContainsKey).Select(id => courseById[id]).ToList();
                    return CreatePagedCourseResponse(fallbackItems, fallbackItems.Count);
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException(
                        $"Academy service error: {httpResponseMsg.StatusCode}",
                        null,
                        httpResponseMsg.StatusCode);
                }
            }

            var pagedCourses = await httpResponseMsg.Content.ReadFromJsonAsync<PagedCourseBulkResponse>(_jsonOptions);

            foreach (var course in pagedCourses?.Items ?? [])
            {
                courseById[course.CourseId] = course;

                var cacheKey = BuildBulkCourseCacheKey(course.CourseId, queryString);
                var cacheString = JsonSerializer.Serialize(course);
                await _distributedCache.SetStringAsync(cacheKey, cacheString, CourseCacheOptions);
            }
        }

        // 3) Return in requested order
        var orderedItems = ids
            .Where(courseById.ContainsKey)
            .Select(id => courseById[id])
            .ToList();

        return CreatePagedCourseResponse(orderedItems, orderedItems.Count);
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

    private static string BuildBulkCourseCacheKey(Guid courseId, string queryString)
    {
        return $"course:bulk:{courseId}:{queryString}";
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

        if (searchRequest.NumberOfParticipation.HasValue)
        {
            queryParts.Add($"NumberOfParticipation={(int)searchRequest.NumberOfParticipation.Value}");
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

