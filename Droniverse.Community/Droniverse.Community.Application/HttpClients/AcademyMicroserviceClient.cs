using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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
    private static readonly DistributedCacheEntryOptions CourseCacheOptions =
    new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
        .SetSlidingExpiration(TimeSpan.FromMinutes(2));
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public AcademyMicroserviceClient(
        HttpClient httpClient,
        ILogger<AcademyMicroserviceClient> logger,
        IDistributedCache distributedCache
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    //public async Task<CourseResponse?> GetCourseById(Guid courseId)
    //{
    //    // ========== 1. READ CACHE ==========
    //    string cacheKey = $"course:{courseId}";
    //    string? cacheCourse = await _distributedCache.GetStringAsync(cacheKey);

    //    if (cacheCourse != null)
    //    {
    //        _logger.LogInformation("Course with id {CourseId} found in cache.", courseId);

    //        var courseFromCache = JsonSerializer.Deserialize<CourseResponse>(cacheCourse);
    //        return courseFromCache ?? throw new KeyNotFoundException($"Course with ID [{courseId}] not found in cache.");
    //    }

    //    // ========== 2. CALL API (bulk nhưng giấu đi) ==========
    //    HttpResponseMessage httpResponseMsg = await _httpClient.PostAsJsonAsync(
    //        "/academy/courses/by-ids?pageIndex=1&pageSize=1",
    //        new { courseIds = new List<Guid> { courseId } }
    //    );

    //    if (!httpResponseMsg.IsSuccessStatusCode)
    //    {
    //        if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
    //        {
    //            _logger.LogError("Academy service unavailable.");
    //            return null;
    //        }
    //        else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
    //        {
    //            _logger.LogWarning("Course with ID [{CourseId}] not found in Academy Microservice.", courseId);
    //            return null;
    //        }
    //        else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
    //        {
    //            throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
    //        }
    //        else
    //        {
    //            throw new HttpRequestException(
    //                $"Academy service error: {httpResponseMsg.StatusCode}",
    //                null,
    //                httpResponseMsg.StatusCode);
    //        }
    //    }

    //    // ========== 3. PARSE RESPONSE ==========
    //    var result = await httpResponseMsg.Content.ReadFromJsonAsync<
    //        SuccessResponse<PaginationResult<IEnumerable<CourseResponse>>>
    //    >();

    //    var course = result?.Data?.Data?.FirstOrDefault();

    //    if (course == null)
    //        throw new ArgumentException("Invalid courseId");

    //    // ========== 4. WRITE CACHE ==========
    //    string cacheString = JsonSerializer.Serialize(course);
    //    await _distributedCache.SetStringAsync(cacheKey, cacheString, CourseCacheOptions);

    //    return course;
    //}

    public async Task<FeedbackResponseDto> GetFeedbackById(Guid feedbackId)
    {
        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/feedbacks/{feedbackId}");
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

    //public async Task<CourseResponse> GetCourseById(Guid courseId)
    //{
    //    try
    //    {
    //        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/courses/{courseId}");
    //        if (!httpResponseMsg.IsSuccessStatusCode)
    //        {
    //            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
    //            {
    //                _logger.LogWarning("Academy service is unavailable.");
    //                return null;
    //            }
    //            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
    //            {
    //                _logger.LogWarning("Course with ID {CourseId} not found in Academy Microservice.", courseId);
    //                return null;
    //            }
    //            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
    //            {
    //                throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
    //            }
    //            else
    //            {
    //                throw new HttpRequestException($"Academy service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
    //            }
    //        }

    //        CourseResponse? course = await httpResponseMsg.Content.ReadFromJsonAsync<CourseResponse>();
    //        if (course == null)
    //        {
    //            throw new ArgumentException("Invalid courseID");
    //        }
    //        return course;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error fetching course with ID {CourseId} from Academy service.", courseId);
    //        throw;
    //    }
    //}

    public async Task<bool> IsLabExist(Guid labId)
    {
        try
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/labs/{labId}");

            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Lab with ID {LabId} not found in Academy Microservice.", labId);
                return false;
            }

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                _logger.LogWarning("Academy service error when checking lab {LabId}: {StatusCode}", labId, httpResponseMsg.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking lab existence with ID {LabId} from Academy service.", labId);
            return false;
        }
    }

    public async Task<bool> IsCertificateExist(Guid certificateId)
    {
        try
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/certificates/{certificateId}");

            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Certificate with ID [{CertificateId}] not found in Academy Microservice.", certificateId);
                return false;
            }

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                _logger.LogWarning("Academy service error when checking certificate {CertificateId}: {StatusCode}", certificateId, httpResponseMsg.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking certificate existence with ID {CertificateId} from Academy service.", certificateId);
            return false;
        }
    }

    public async Task<CertificateDetailResponse?> GetCertificateById(Guid certificateId)
    {
        try
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/certificates/{certificateId}");

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

    public async Task<IEnumerable<CertificateDetailResponse>> GetCertificatesBulk(IEnumerable<Guid> certificateIds)
    {
        if (certificateIds == null || !certificateIds.Any())
            return Enumerable.Empty<CertificateDetailResponse>();

        var distinctIds = certificateIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/api/certificates/bulk",
                distinctIds
            );

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service unavailable (certificates bulk request).");
                    return Enumerable.Empty<CertificateDetailResponse>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogError("Bad request when calling Academy certificates bulk API.");
                    return Enumerable.Empty<CertificateDetailResponse>();
                }

                _logger.LogError("Academy certificates bulk API error: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<CertificateDetailResponse>();
            }

            var certificates = await response.Content
                                      .ReadFromJsonAsync<IEnumerable<CertificateDetailResponse>>();

            return certificates ?? Enumerable.Empty<CertificateDetailResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Academy certificates bulk API");
            return Enumerable.Empty<CertificateDetailResponse>();
        }
    }

    public async Task<IEnumerable<CourseBulkResponseDTO>> GetCourseById(
        IEnumerable<Guid> courseIds,
        CourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new CourseBulkSearchRequest();

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

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
                //$"/academy/courses/by-ids?{queryString}",
                $"/api/academy/courses/by-ids?{queryString}",
                new GetCoursesByIdsRequestDTO { CourseIds = missingIds });

            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("Academy service unavailable.");
                    return ids.Where(courseById.ContainsKey).Select(id => courseById[id]).ToList();
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Courses not found in Academy Microservice.");
                    return ids.Where(courseById.ContainsKey).Select(id => courseById[id]).ToList();
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

            var courses = await httpResponseMsg.Content.ReadFromJsonAsync<IEnumerable<CourseBulkResponseDTO>>(_jsonOptions) ?? [];

            foreach (var course in courses)
            {
                courseById[course.CourseId] = course;

                var cacheKey = BuildBulkCourseCacheKey(course.CourseId, queryString);
                var cacheString = JsonSerializer.Serialize(course);
                await _distributedCache.SetStringAsync(cacheKey, cacheString, CourseCacheOptions);
            }
        }

        // 3) Return in requested order
        return ids
            .Where(courseById.ContainsKey)
            .Select(id => courseById[id])
            .ToList();
    }

    private static string BuildBulkCourseCacheKey(Guid courseId, string queryString)
    {
        return $"course:bulk:{courseId}:{queryString}";
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

