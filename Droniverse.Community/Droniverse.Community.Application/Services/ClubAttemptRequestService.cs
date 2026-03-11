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
                UserResponse approver = null;
                if (clubRequest.ApproverID != null)
                    userDict.TryGetValue((Guid)clubRequest.ApproverID, out approver);

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.Club?.NameVN,
                    clubRequest.Club?.NameEN,
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
                ClubNameVN = request.Club?.NameVN,
                ClubNameEN = request.Club?.NameEN,
                Status = request.Status,
                ProcessedAt = request.ProcessedAt,
                ParticipationID = participationId
            };

            return response;
        }

        public async Task<IEnumerable<ClubRequestResponseDto>> GetClubAttemptRequestsByRequester()
        {
            var requesterID = Guid.Parse(_currentUserService.UserID
                ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var clubRequests = await _unitOfWork.ClubAttemptRequests
                .GetManyByCondition(
                    c => c.RequesterID == requesterID,
                    query => query.Include(c => c.Club)
                );

            if (clubRequests == null || !clubRequests.Any())
                return Enumerable.Empty<ClubRequestResponseDto>();

            var userIds = clubRequests
                .Select(x => x.RequesterID)
                .Union(clubRequests.Where(x => x.ApproverID.HasValue).Select(x => x.ApproverID.Value))
                .Distinct()
                .ToList();

            var users = await _identityMicroserviceClient.GetUsersBulk(userIds);
            var userDict = users.ToDictionary(u => u.UserId, u => u);

            var result = clubRequests.Select(clubRequest =>
            {
                userDict.TryGetValue(clubRequest.RequesterID, out var requester);
                UserResponse approver = null;
                if (clubRequest.ApproverID != null)
                    userDict.TryGetValue(clubRequest.ApproverID.Value, out approver);

                return new ClubRequestResponseDto(
                    clubRequest.ClubRequestID,
                    clubRequest.RequesterID,
                    clubRequest.ApproverID,
                    clubRequest.ClubID,
                    clubRequest.Club?.NameVN,
                    clubRequest.Club?.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    clubRequest.Status,
                    clubRequest.CreatedAt,
                    clubRequest.ProcessedAt
                );
            });

            return result;
        }

        public async Task<PaginationResult<IEnumerable<ClubRequestResponseDto>>> GetAllClubAttemptRequests(ClubAttemptRequestSearchRequest searchRequest)
        {
            // Calculate pagination parameters
            var skip = (searchRequest.CurrentPage - 1) * searchRequest.PageSize;
            var take = searchRequest.PageSize;

            // Use optimized repository method - single DB call with pagination
            var (requests, totalCount) = await _unitOfWork.ClubAttemptRequests.GetFilteredRequestsAsync(
                status: searchRequest.Status,
                createdFrom: searchRequest.CreatedFrom,
                createdTo: searchRequest.CreatedTo,
                processedFrom: searchRequest.ProcessedFrom,
                processedTo: searchRequest.ProcessedTo,
                sortBy: searchRequest.SortBy,
                sortDirection: searchRequest.SortDirection,
                skip: skip,
                take: take
            );

            // Early return if no data
            if (!requests.Any())
            {
                return new PaginationResult<IEnumerable<ClubRequestResponseDto>>(
                    Enumerable.Empty<ClubRequestResponseDto>(),
                    totalRecords: 0,
                    pageIndex: searchRequest.CurrentPage,
                    pageSize: searchRequest.PageSize
                );
            }

            // Get unique user IDs for bulk fetch - optimized
            var userIds = requests
                .SelectMany(r => new[] { r.RequesterID, r.ApproverID })
                .Where(id => id.HasValue && id.Value != Guid.Empty)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            // Single call to Identity service for all users
            var users = userIds.Any() 
                ? await _identityMicroserviceClient.GetUsersBulk(userIds)
                : Enumerable.Empty<UserResponse>();
            
            var userDict = users.ToDictionary(u => u.UserId, u => u);

            // Map to response DTOs - in-memory operation
            var responseDtos = requests.Select(request =>
            {
                userDict.TryGetValue(request.RequesterID, out var requester);
                UserResponse? approver = null;
                if (request.ApproverID.HasValue)
                {
                    userDict.TryGetValue(request.ApproverID.Value, out approver);
                }

                return new ClubRequestResponseDto(
                    request.ClubRequestID,
                    request.RequesterID,
                    request.ApproverID,
                    request.ClubID,
                    request.Club?.NameVN,
                    request.Club?.NameEN,
                    requester?.LastName,
                    approver?.LastName,
                    request.Status,
                    request.CreatedAt,
                    request.ProcessedAt
                );
            }).ToList();

            // Return paginated result with pre-calculated total
            return new PaginationResult<IEnumerable<ClubRequestResponseDto>>(
                responseDtos,
                totalCount,
                searchRequest.CurrentPage,
                searchRequest.PageSize
            );
        }
    }
}
