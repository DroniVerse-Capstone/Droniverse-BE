using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Droniverse.Academy.Application.HttpClients.IdentityService;

internal sealed class SystemConfigClient : IdentityBaseClient
{
    public SystemConfigClient(
        HttpClient httpClient,
        ILogger logger,
        ICacheService cacheService,
        IHostEnvironment environment)
        : base(httpClient, logger, cacheService, environment)
    {
    }

    public async Task<CertificateTemplateResponse?> GetCertificateTemplate()
    {
        try
        {
            var response = await HttpClient.GetAsync(BuildIdentityPath("system-configs/certificate"));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Logger.LogError("Identity service unavailable when getting certificate template.");
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("Certificate template not found.");
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException(
                        "Bad request when calling certificate template API",
                        null,
                        HttpStatusCode.BadRequest);
                }

                throw new HttpRequestException(
                    $"Identity service error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<CertificateTemplateResponse>(JsonOptions);

            if (result == null)
            {
                throw new ArgumentException("Invalid certificate template response");
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling certificate template API");
            throw;
        }
    }

    public async Task<SystemEstimatetime?> GetSystemEstimatetimeAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync(BuildIdentityPath("system-configs/estimatetime"));
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Logger.LogError("Identity service unavailable when getting system estimatetime.");
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning("System estimatetime not found.");
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException(
                        "Bad request when calling system estimatetime API",
                        null,
                        HttpStatusCode.BadRequest);
                }

                throw new HttpRequestException(
                    $"Identity service error: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var result = await response.Content.ReadFromJsonAsync<SystemEstimatetime>(JsonOptions);
            if (result == null)
            {
                throw new ArgumentException("Invalid system estimatetime response");
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling system estimatetime API");
            throw;
        }
    }
}
