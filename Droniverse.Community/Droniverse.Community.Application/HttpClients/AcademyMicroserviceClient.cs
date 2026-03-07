using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

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
    //private readonly IDistributedCache _distributedCache; //Redis Cache
    public AcademyMicroserviceClient(
        HttpClient httpClient,
        ILogger<AcademyMicroserviceClient> logger
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        
    }

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

    public async Task<CourseResponse> GetCourseById(Guid courseId)
    {
        try
        {
            HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/courses/{courseId}");
            if (!httpResponseMsg.IsSuccessStatusCode)
            {
                if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogWarning("Academy service is unavailable.");
                    return null;
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found in Academy Microservice.", courseId);
                    return null;
                }
                else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Academy service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
                }
            }

            CourseResponse? course = await httpResponseMsg.Content.ReadFromJsonAsync<CourseResponse>();
            if (course == null)
            {
                throw new ArgumentException("Invalid courseID");
            }
            return course;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching course with ID {CourseId} from Academy service.", courseId);
            throw;
        }
    }

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
                _logger.LogWarning("Certificate with ID {CertificateId} not found in Academy Microservice.", certificateId);
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

