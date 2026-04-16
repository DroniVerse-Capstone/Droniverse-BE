using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.HttpClients.IdentityService;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.HttpClients
{
    public class IdentityMicroserviceClient
    {
        private readonly UserClient _userClient;
        private readonly SystemConfigClient _systemConfigClient;

        public IdentityMicroserviceClient(
            HttpClient httpClient,
            ILogger<IdentityMicroserviceClient> logger,
            ICacheService cacheService,
            IHostEnvironment environment
            )
        {
            _userClient = new UserClient(httpClient, logger, cacheService, environment);
            _systemConfigClient = new SystemConfigClient(httpClient, logger, cacheService, environment);
        }

        public async Task<SimpleUserReponse?> GetUserByUserID(Guid userId)
        {
            return await _userClient.GetUserByUserID(userId);
        }

        public async Task<IEnumerable<UserResponse>> GetUsersBulk(IEnumerable<Guid> userIds)
        {
            return await _userClient.GetUsersBulk(userIds);
        }

        public async Task<IEnumerable<Guid>> GetUserIdsBySearchName(UserInfoSearchRequestDTO request)
        {
            return await _userClient.GetUserIdsBySearchName(request);
        }

        public async Task<SearchUsersWithPaginationResponse> SearchUsersWithPaginationAsync(
         SearchUsersWithPaginationRequest request,
         CancellationToken cancellationToken = default)
        {
            return await _userClient.SearchUsersWithPaginationAsync(request, cancellationToken);
        }


        public async Task<CertificateTemplateResponse?> GetCertificateTemplate()
        {
            return await _systemConfigClient.GetCertificateTemplate();
        }

        public async Task<SystemEstimatetime?> GetSystemEstimatetimeAsync()
        {
            return await _systemConfigClient.GetSystemEstimatetimeAsync();
        }
    }
}
