using DnsClient.Internal;
using Droniverse.Shared.DTOs.Response;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Droniverse.Community.Application.HttpClients;

public class IdentityMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityMicroserviceClient> _logger;
    //private readonly IDistributedCache _distributedCache; //Redis Cache
    public IdentityMicroserviceClient(
        HttpClient httpClient,
        ILogger<IdentityMicroserviceClient> logger
        )
    {
        _httpClient = httpClient;
        _logger = logger;
        //_distributedCache = distributedCache;
    }

    public async Task<UserResponse> GetUserByUserID(Guid userId)
    {

        //Read from cache
        //key:value
        //userid:{object} ttl:30p

        HttpResponseMessage httpResponseMsg = await _httpClient.GetAsync($"/api/users/{userId}");
        if (!httpResponseMsg.IsSuccessStatusCode)
        {
            if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {

            }

            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User with ID {UserId} not found in Identity Microservice.", userId);
                return null;
            }
            else if (httpResponseMsg.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                //fallback data
                throw new HttpRequestException($"Identity service error: {httpResponseMsg.StatusCode}", null, httpResponseMsg.StatusCode);
            }
        }
        UserResponse? user = await httpResponseMsg.Content.ReadFromJsonAsync<UserResponse>();
        if (user == null)
        {
            throw new ArgumentException("Invalid userID");

        }

        //Write to cache
        //key:value


        return user;

    }
}

