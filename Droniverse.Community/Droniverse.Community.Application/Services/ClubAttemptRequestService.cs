using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class ClubAttemptRequestService : IClubAttemptRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;

        public ClubAttemptRequestService(IUnitOfWork unitOfWork, IMapper mapper, IdentityMicroserviceClient identityMicroserviceClient, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
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

            ClubAttemptRequest clubRequest = new(clubID, requesterID);
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
                    query => query.Include(c => c.Club)
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

            var users = await _identityMicroserviceClient.GetUsersBulk((IEnumerable<Guid>)userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => u);
            var result = clubRequests.Select(clubRequest =>
            {
                userDict.TryGetValue(clubRequest.RequesterID, out var requester);

                var approver = clubRequest.ApproverID.HasValue && userDict.TryGetValue((Guid)clubRequest.ApproverID, out var a) ? a : null;

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.Club.NameVN,
                    clubRequest.Club.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    clubRequest.Status,
                    clubRequest.CreatedAt,
                    clubRequest.ProcessedAt
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
            );

            if (request == null)
                throw new InvalidOperationException($"Club attempt request with ID {id} not found.");

            Guid? participationId = null;

            switch (dto.Status)
            {
                case ClubAttemptRequestStatus.APPROVED:
                    request.Approve(approverId);

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
                ClubNameVN = request.Club.NameVN,
                ClubNameEN = request.Club.NameEN,
                Status = request.Status,
                ProcessedAt = request.ProcessedAt,
                ParticipationID = participationId
            };

            return response;
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
                    query => query.Include(c => c.Club).OrderByDescending(c => c.CreatedAt)
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

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.Club.NameVN,
                    clubRequest.Club.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    clubRequest.Status,
                    clubRequest.CreatedAt,
                    clubRequest.ProcessedAt
                );
            });

            return result;
        }

        public async Task<PaginationResult<IEnumerable<ClubRequestResponseDto>>> GetAllClubAttemptRequests(
    ClubAttemptRequestSearchRequest searchRequest)
        {
            var skip = (searchRequest.CurrentPage - 1) * searchRequest.PageSize;
            var take = searchRequest.PageSize;

            var (requests, totalCount) = await _unitOfWork.ClubAttemptRequests.GetFilteredRequestsAsync(
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

            IEnumerable<UserResponse> users = Enumerable.Empty<UserResponse>();

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

                return new ClubRequestResponseDto(
                    request.ClubRequestID,
                    request.RequesterID,
                    request.ApproverID,
                    request.ClubID,
                    request.Club.NameVN,
                    request.Club.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    request.Status,
                    request.CreatedAt,
                    request.ProcessedAt
                );
            }).ToList();

            return responseDtos.ToPaginationResult(searchRequest);
        }
    }
}
