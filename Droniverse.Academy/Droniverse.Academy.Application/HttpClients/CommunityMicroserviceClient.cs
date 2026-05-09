using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients.CommunityService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.HttpClients
{
    public class CommunityMicroserviceClient
    {
        private readonly ClubClient _clubClient;
        private readonly ClubCourseClient _clubCourseClient;
        private readonly MediaClient _mediaClient;
        private readonly ProductClient _productClient;

        public CommunityMicroserviceClient(HttpClient httpClient, ILogger<CommunityMicroserviceClient> logger, ICacheService cacheService, IHostEnvironment environment)
        {
            _clubClient = new ClubClient(httpClient, logger, cacheService, environment);
            _clubCourseClient = new ClubCourseClient(httpClient, logger, cacheService, environment);
            _mediaClient = new MediaClient(httpClient, logger, cacheService, environment);
            _productClient = new ProductClient(httpClient, logger, cacheService, environment);
        }

        public async Task<ClubCourseResponse> AddCourseToClub(Guid clubId, AddClubCourseRequestDto request)
        {
            return await _clubCourseClient.AddCourseToClub(clubId, request);
        }

        public async Task<IEnumerable<DroneResponseDto>> GetMyClubsAsync()
        {
            return await _clubClient.GetMyClubsAsync();

        }

        /// <summary>
        /// Khi club_member
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="courseId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<bool> ConsumeSlotForCodeAsync(
            Guid clubId,
            Guid courseId,
            int num,
            CancellationToken cancellationToken = default)
        {
            return await _clubCourseClient.ConsumeSlotForCodeAsync(clubId, courseId, num, cancellationToken);
        }



        public async Task<ClubCourseOwn?> GetRemainingQuantityAsync(
            Guid clubId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await _clubCourseClient.GetRemainingQuantityAsync(clubId, courseId, cancellationToken);
        }

        public async Task<ProductMiniResponseDTO?> GetProductByReferenceIdAsync(
            Guid referenceId,
            CancellationToken cancellationToken = default)
        {
            return await _productClient.GetProductByReferenceIdAsync(referenceId, cancellationToken);
        }

        public async Task<IEnumerable<ProductMiniResponseDTO>> GetProductsBulkByReferenceIdsAsync(
            IEnumerable<Guid> referenceIds,
            CancellationToken cancellationToken = default)
        {
            return await _productClient.GetProductsBulkByReferenceIdsAsync(referenceIds, cancellationToken);
        }

        public async Task<IEnumerable<MediaMiniResponse>> GetMiniResponse(
            IEnumerable<Guid> mediaIds,
            CancellationToken cancellationToken = default)
        {
            return await _mediaClient.GetMiniResponse(mediaIds, cancellationToken);
        }

        public async Task<IEnumerable<SimpleClubResponse>> GetClubInfoBulkAsync(
        IEnumerable<Guid> clubIds,
        CancellationToken cancellationToken = default)
        {
            return await _clubClient.GetClubInfoBulkAsync(clubIds, cancellationToken);
        }

        public async Task<ClubCourseRemainingQuantityResponseDto?> GetRemainingQuantityRawAsync(
            Guid clubId,
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await _clubCourseClient.GetRemainingQuantityRawAsync(clubId, courseId, cancellationToken);
        }

        public async Task<List<Guid>> GetClubParticipantIdsAsync(
                    Guid clubId,
                    ParticipationStatus status = ParticipationStatus.ACTIVE,
                    CancellationToken cancellationToken = default)
        {
            return await _clubClient.GetClubParticipantIdsAsync(clubId, status, cancellationToken);
        }

        public async Task<ClubCourseResponseDto?> ConsumeSlotCrossAsync(
                Guid clubId,
                Guid courseId,
                int quantity = 1,
                CancellationToken cancellationToken = default)
        {
            return await _clubCourseClient.ConsumeSlotCrossAsync(clubId, courseId, quantity, cancellationToken);
        }

        public async Task<bool> CheckParticipantByClubAsync(
                Guid clubId,
                Guid userId,
                ParticipationStatus status = ParticipationStatus.ACTIVE,
                CancellationToken cancellationToken = default)
        {
            return await _clubClient.CheckParticipantByClubAsync(clubId, userId, status, cancellationToken);
        }

        public async Task<Guid> GetDroneFromClubAsync(
            Guid clubId,
            CancellationToken cancellationToken = default)
        {
            return await _clubClient.GetDroneFromClubAsync(clubId, cancellationToken);
        }

        public async Task<IEnumerable<ClubMiniResponseDto>> GetClubMiniBulkAsync(
            IEnumerable<Guid> clubIds,
            CancellationToken cancellationToken = default)
        {
            return await _clubClient.GetClubMiniBulkAsync(clubIds, cancellationToken);
        }

        public async Task<MediaMiniResponse?> GetMediaMiniResponsesAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            return await _mediaClient.GetMediaByUrl(url, cancellationToken);

        }
    }
}
