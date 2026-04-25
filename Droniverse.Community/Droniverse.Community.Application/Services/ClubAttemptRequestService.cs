using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.Helpers;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services
{
    public class ClubAttemptRequestService : IClubAttemptRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;

        public ClubAttemptRequestService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient, AcademyMicroserviceClient academyMicroserviceClient, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
            _academyMicroserviceClient = academyMicroserviceClient;
            _currentUserService = currentUserService;
        }

        public async Task CreateAttemptClubRequest(Guid clubID)
        {
            var requesterID = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID);
            if (club == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy club với ID {clubID}");
            }

            Guid? mediaID = null;
            ClubAttemptRequest clubRequest = new(requesterID, clubID, mediaID, null);
            await _unitOfWork.ClubAttemptRequests.Add(clubRequest);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByID(Guid clubID)
        {
            if (clubID == Guid.Empty)
                throw new ArgumentException("ClubID không được để trống", nameof(clubID));

            var clubRequests = await _unitOfWork.ClubAttemptRequests
                .GetManyByCondition(
                    c => c.ClubID == clubID,
                    query => query.AsNoTracking().Include(c => c.Club)
                );

            if (clubRequests == null)
                throw new KeyNotFoundException();

            if (!clubRequests.Any())
                return Enumerable.Empty<ClubRequestResponseDto>();

            var userIds = clubRequests
                .SelectMany(x => new[] { x.RequesterID, x.ApproverID })
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            IEnumerable<UserResponse> users = await _identityMicroserviceClient.GetUsersBulk((IEnumerable<Guid>)userIds);
            Dictionary<Guid, UserResponse> userDict = users.ToDictionary(u => u.UserId, u => u);

            var result = clubRequests.Select(clubRequest =>
            {
                userDict.TryGetValue(clubRequest.RequesterID, out var requester);
                var approver = clubRequest.ApproverID.HasValue && userDict.TryGetValue((Guid)clubRequest.ApproverID, out var a) ? a : null;
                var userLevel = requester.UserLevel;
                var userLevelMax = requester.UserLevelMax;

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.ClubRequirement,
                    clubRequest.Club.NameVN,
                    clubRequest.Club.NameEN,
                    clubRequest.Club.ImageUrl,
                    AppHelper.GetFullName(requester),
                    requester?.Email,
                    AppHelper.GetFullName(approver),
                    approver?.Email,
                    clubRequest.Status,
                    clubRequest.CreatedAt,
                    clubRequest.ProcessedAt,
                    _mapper.Map<MediaResponseDto?>(clubRequest.MediaID.HasValue ? clubRequest.Media : null),
                    userLevel,
                    userLevelMax

                );
            });

            return result;
        }

        public async Task<ClubAttemptRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubAttemptRequestUpdateStatusDto dto)
        {
            var approverId = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var request = await _unitOfWork.ClubAttemptRequests.GetByCondition(
                r => r.ClubRequestID == id,
                q => q.Include(r => r.Club)
                .Include(c => c.Media)
            );

            if (request == null)
                throw new InvalidOperationException($"Club attempt request with ID {id} not found.");

            Guid? participationId = null;

            try
            {
                switch (dto.Status)
                {
                    case ClubAttemptRequestStatus.APPROVED:
                        request.Approve(approverId);

                        // Check if user already exists in club
                        var existingParticipation = await _unitOfWork.Participations.GetByCondition(
                            p => p.ClubID == request.ClubID && p.UserID == request.RequesterID);
                        
                        if (existingParticipation != null)
                            throw new InvalidOperationException($"User {request.RequesterID} already exists in club {request.ClubID}");

                        var participation = new Participation(
                            request.RequesterID,
                            request.ClubID,
                            approverId
                        );

                        await _unitOfWork.Participations.Add(participation);
                        participationId = participation.ParticipationID;
                        break;

                    case ClubAttemptRequestStatus.REJECT:
                        request.Reject(approverId);
                        break;

                    case ClubAttemptRequestStatus.PENDING:
                        request.ResetToPending();
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot update to status {dto.Status}.");
                }

                await _unitOfWork.ClubAttemptRequests.Update(request);
                await _unitOfWork.SaveChangeAsync();

                var response = new ClubAttemptRequestUpdateStatusResponseDto
                {
                    ClubRequestID = request.ClubRequestID,
                    RequesterID = request.RequesterID,
                    ClubID = request.ClubID,
                    ClubRequirement = request.ClubRequirement,
                    ClubNameVN = request.Club.NameVN,
                    ClubNameEN = request.Club.NameEN,
                    Status = request.Status,
                    ProcessedAt = request.ProcessedAt,
                    ParticipationID = participationId
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating club attempt request: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByRequester(ClubAttemptRequestStatus? status)
        {
            var requesterID = Guid.Parse(
                _currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực.")
            );

            var clubRequests = await _unitOfWork.ClubAttemptRequests
                .GetManyByCondition(
                    c => c.RequesterID == requesterID &&
                         (!status.HasValue || c.Status == status),
                    query => query.AsNoTracking().Include(c => c.Club).Include(c => c.Media).OrderByDescending(c => c.CreatedAt)
                );

            if (clubRequests == null || !clubRequests.Any())
                return [];

            var userIds = clubRequests
                .Select(x => x.RequesterID)
                .Union(clubRequests
                    .Select(x => x.ApproverID)
                    .OfType<Guid>()
                )
                .Distinct()
                .ToList();

            IEnumerable<UserResponse> users = [];

            try
            {
                if (userIds.Any())
                    users = await _identityMicroserviceClient.GetUsersBulk(userIds);
            }
            catch
            {
                Console.WriteLine("Không lấy được thông tin user từ identity service.");
            }

            var userDict = users.ToDictionary(u => u.UserId, u => u);

            var result = clubRequests
                .Where(c => c != null)
                .Select(clubRequest =>
                {
                    userDict.TryGetValue(clubRequest.RequesterID, out var requester);

                    var approver = clubRequest.ApproverID.HasValue &&
                                   userDict.TryGetValue(clubRequest.ApproverID.Value, out var a)
                                   ? a
                                   : null;
                    var userLevel = requester.UserLevel;
                    var userLevelMax = requester.UserLevelMax;

                    return new ClubRequestResponseDto(
                        clubRequest.ClubRequestID,
                        clubRequest.RequesterID,
                        clubRequest.ApproverID,
                        clubRequest.ClubID,
                        clubRequest.ClubRequirement,
                        clubRequest.Club.NameVN,
                        clubRequest.Club.NameEN,
                        clubRequest.Club.ImageUrl,
                        AppHelper.GetFullName(requester),
                        requester?.Email,
                        AppHelper.GetFullName(approver),
                        approver?.Email,
                        clubRequest.Status,
                        clubRequest.CreatedAt,
                        clubRequest.ProcessedAt,
                        _mapper.Map<MediaResponseDto?>(clubRequest.MediaID.HasValue ? clubRequest.Media : null),
                        userLevel,
                        userLevelMax
                    );
                });

            return result;
        }

        public async Task<PaginationResult<IEnumerable<ClubRequestResponseDto>>> GetAllClubAttemptRequests(Guid clubID,
    ClubAttemptRequestSearchRequest searchRequest)
        {
            var skip = (searchRequest.CurrentPage - 1) * searchRequest.PageSize;
            var take = searchRequest.PageSize;

            var (requests, totalCount) = await _unitOfWork.ClubAttemptRequests.GetFilteredRequestsAsync(
                clubID,
                searchRequest.Status,
                searchRequest.CreatedFrom,
                searchRequest.CreatedTo,
                searchRequest.ProcessedFrom,
                searchRequest.ProcessedTo,
                searchRequest.SortBy,
                searchRequest.SortDirection,
                skip,
                take
            );

            if (requests?.Any() != true)
                return Enumerable.Empty<ClubRequestResponseDto>().ToPaginationResult(searchRequest);

            var userIds = requests
                .Select(r => r.RequesterID)
                .Union(requests.Select(r => r.ApproverID).OfType<Guid>())
                .Distinct()
                .ToList();

            IEnumerable<UserResponse> users = [];

            if (userIds.Any())
            {
                try
                {
                    users = await _identityMicroserviceClient.GetUsersBulk(userIds);
                }
                catch
                {
                    Console.WriteLine("Identity service unavailable");
                }
            }

            var userDict = users.ToDictionary(u => u.UserId);


            var responseDtos = requests.Select(request =>
            {
                userDict.TryGetValue(request.RequesterID, out var requester);

                UserResponse? approver = null;
                if (request.ApproverID.HasValue)
                    userDict.TryGetValue(request.ApproverID.Value, out approver);

                IEnumerable<UserLevelResponseDto>? userLevel = requester.UserLevel;
                IEnumerable<UserLevelResponseDto>? userLevelMax = requester.UserLevelMax;

                return new ClubRequestResponseDto(
                    request.ClubRequestID,
                    request.RequesterID,
                    request.ApproverID,
                    request.ClubID,
                    request.ClubRequirement,
                    request.Club.NameVN,
                    request.Club.NameEN,
                    request.Club.ImageUrl,
                    AppHelper.GetFullName(requester),
                    requester?.Email,
                    AppHelper.GetFullName(approver),
                    approver?.Email,
                    request.Status,
                    request.CreatedAt,
                    request.ProcessedAt,
                    _mapper.Map<MediaResponseDto?>(request.MediaID.HasValue ? request.Media : null),
                    userLevel,
                    userLevelMax
                );
            }).ToList();

            return responseDtos.ToPaginationResult(searchRequest);
        }

    }
}