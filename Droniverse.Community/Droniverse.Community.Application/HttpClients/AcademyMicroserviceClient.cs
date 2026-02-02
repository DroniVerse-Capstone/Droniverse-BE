
using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Droniverse.Community.Application.HttpClients;

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
    
}

