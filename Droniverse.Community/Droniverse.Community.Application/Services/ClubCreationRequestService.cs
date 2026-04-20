using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using Droniverse.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Shared.Helpers;

namespace Droniverse.Community.Application.Services
{
    public class ClubCreationRequestService : IClubCreationRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IdentityMicroserviceClient _identityMicroserviceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public ClubCreationRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IdentityMicroserviceClient identityMicroserviceClient,
            ICurrentUserService currentUserService,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _identityMicroserviceClient = identityMicroserviceClient;
            _currentUserService = currentUserService;
            _clock = clock;
        }

        public async Task<ClubCreationRequestCreateResponseDto> CreateRequestToCreateClub(ClubCreationRequestCreateDto dto)
        {
            var requesterID = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var isUserExisted = await _unitOfWork.ClubCreationRequests.IsUserHavingOtherRequest(requesterID);
            if (isUserExisted)
                throw new InvalidOperationException("Người dùng hiện đang có một yêu cầu khác chưa xử lí xong. Không thể tạo mới được");

            var media = await _unitOfWork.Medias.GetByCondition(m => m.MediaID == dto.Media);
            if (media == null)
                throw new NotFoundException($"Media (hình ảnh/video) không tồn tại trong hệ thống temp.");

            var request = new ClubCreationRequest(
                dto.NameVN,
                dto.NameEN,
                dto.Description,
                dto.IsPublic,
                dto.LimitParticipant,
                1, //limit club manager mặc định là 1
                dto.Image,
                requesterID,
                dto.DroneID,
                dto.Media,
                dto.ClubPolicy
            );

            await _unitOfWork.ClubCreationRequests.Add(request);

            await _unitOfWork.SaveChangeAsync();

            return new ClubCreationRequestCreateResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameEN = request.NameEN,
                NameVN = request.NameVN
            };
        }

        public async Task<PaginationResult<IEnumerable<ClubCreationRequestResponseDto>>> GetAllClubCreationRequest(ClubCreationRequestSearchRequest searchRequest)
        {
            var requests = await _unitOfWork.ClubCreationRequests.GetManyByCondition(
                                        x => !searchRequest.status.HasValue || x.Status == searchRequest.status.Value,
                                        q => q.Include(x => x.Media).AsNoTracking().OrderByDescending(c => c.CreatedAt)
                                    );

            if (requests == null || !requests.Any())
                return Enumerable.Empty<ClubCreationRequestResponseDto>().ToPaginationResult(searchRequest);

            var userIds = requests
                .Select(x => x.RequesterID)
                .Union(requests.Select(x => x.ApproverID).OfType<Guid>())
                .Distinct()
                .ToList();

            IEnumerable<UserResponse> users = Enumerable.Empty<UserResponse>();
            if (userIds.Count != 0)
            {
                try
                {
                    users = await _identityMicroserviceClient.GetUsersBulk(userIds);
                }
                catch
                {
                    Console.WriteLine("Không lấy được thông tin user từ identity service.");
                }
            }

            var userDict = users.ToDictionary(u => u.UserId, u => u);

            var result = requests.Select(x => 
            {
                userDict.TryGetValue(x.RequesterID, out var requester);
                var approver = x.ApproverID.HasValue && userDict.TryGetValue(x.ApproverID.Value, out var a) ? a : null;

                return new ClubCreationRequestResponseDto
                {
                    ClubCreationRequestID = x.ClubCreationRequestID,
                    NameVN = x.NameVN,
                    NameEN = x.NameEN,
                    Description = x.Description,
                    IsPublic = x.IsPublic,
                    LimitParticipant = x.LimitParticipant,
                    LimitClubManager = x.LimitClubManager,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectReason = x.RejectReason,
                    ClubID = x.ClubID,
                    RequesterID = x.RequesterID,
                    ApproverID = x.ApproverID,
                    RequesterName = AppHelper.GetFullName(requester),
                    RequesterEmail = requester?.Email,
                    ApproverName = AppHelper.GetFullName(approver),
                    ApproverEmail = approver?.Email,
                    Status = x.Status,
                    Media = _mapper.Map<MediaResponseDto>(x.Media)
                };
            });

            return result.ToPaginationResult(searchRequest);
        }

        public async Task<IEnumerable<ClubCreationRequestResponseDto>> GetMyClubCreationRequest(ClubCreationRequestStatus? status = null)
        {
            var managerId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            var requests = await _unitOfWork.ClubCreationRequests.GetManyByCondition(
                                        x => x.RequesterID == managerId && (!status.HasValue || x.Status == status.Value),
                                        q => q.OrderByDescending(c => c.CreatedAt)
                                    );

            if (requests == null || !requests.Any())
                return [];

            var userIds = requests
                .Select(x => x.RequesterID)
                .Union(requests.Select(x => x.ApproverID).OfType<Guid>())
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
                    Console.WriteLine("Không lấy được thông tin user từ identity service.");
                }
            }

            var userDict = users.ToDictionary(u => u.UserId, u => u);

            return requests.Select(x =>
            {
                userDict.TryGetValue(x.RequesterID, out var requester);
                var approver = x.ApproverID.HasValue && userDict.TryGetValue(x.ApproverID.Value, out var a) ? a : null;

                return new ClubCreationRequestResponseDto
                {
                    ClubCreationRequestID = x.ClubCreationRequestID,
                    NameVN = x.NameVN,
                    NameEN = x.NameEN,
                    Description = x.Description,
                    IsPublic = x.IsPublic,
                    LimitParticipant = x.LimitParticipant,
                    LimitClubManager = x.LimitClubManager,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    ApprovedAt = x.ApprovedAt,
                    RejectReason = x.RejectReason,
                    ClubID = x.ClubID,
                    RequesterID = x.RequesterID,
                    ApproverID = x.ApproverID,
                    RequesterName = AppHelper.GetFullName(requester),
                    RequesterEmail = requester?.Email,
                    ApproverName = AppHelper.GetFullName(approver),
                    ApproverEmail = approver?.Email,
                    Status = x.Status
                };
            });
        }

        public async Task<ClubCreationRequestResponseDto> GetClubCreationRequestById(Guid id)
        {
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(
                                        x => x.ClubCreationRequestID == id,
                                        q => q.Include(x => x.Media));

            if (request == null)
                throw new KeyNotFoundException($"Club creation request with ID [{id}] not found.");

            var userIds = new List<Guid> { request.RequesterID };
            if (request.ApproverID.HasValue)
                userIds.Add(request.ApproverID.Value);

            IEnumerable<UserResponse> users = Enumerable.Empty<UserResponse>();
            try
            {
                users = await _identityMicroserviceClient.GetUsersBulk(userIds.Distinct());
            }
            catch
            {
                Console.WriteLine("Không lấy được thông tin user từ identity service.");
            }

            var userDict = users.ToDictionary(u => u.UserId, u => u);
            userDict.TryGetValue(request.RequesterID, out var requester);
            var approver = request.ApproverID.HasValue && userDict.TryGetValue(request.ApproverID.Value, out var a) ? a : null;

            return new ClubCreationRequestResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameVN = request.NameVN,
                NameEN = request.NameEN,
                Description = request.Description,
                IsPublic = request.IsPublic,
                LimitParticipant = request.LimitParticipant,
                LimitClubManager = request.LimitClubManager,
                ImageUrl = request.ImageUrl,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ApprovedAt = request.ApprovedAt,
                RejectReason = request.RejectReason,
                ClubID = request.ClubID,
                RequesterID = request.RequesterID,
                ApproverID = request.ApproverID,
                RequesterName = AppHelper.GetFullName(requester),
                RequesterEmail = requester?.Email,
                ApproverName = AppHelper.GetFullName(approver),
                ApproverEmail = approver?.Email,
                Status = request.Status,
                Media = _mapper.Map<MediaResponseDto>(request.Media)
            };
        }

        public async Task<ClubCreationRequestUpdateStatusResponseDto> UpdateRequestStatus(Guid id, ClubCreationRequestUpdateStatusDto dto)
        {
            var approverId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));
            var roles = _currentUserService.Roles.ToList();

            // Get the request from database
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(r => r.ClubCreationRequestID == id,
                    query => query
                );

            if (request == null)
                throw new InvalidOperationException($"Club creation request with ID {id} not found.");

            bool isAdmin = roles.Contains(Roles.Admin);
            bool isSystemManager = roles.Contains(Roles.SystemManager);
            bool isRequester = request.RequesterID == approverId;

            switch (dto.Status)
            {
                case ClubCreationRequestStatus.APPROVED:
                    if (!isAdmin && !isSystemManager)
                        throw new ForbiddenException("Chỉ SYSTEM_MANAGER hoặc ADMIN mới có quyền approve.");

                    var newClub = new Club(
                        request.NameVN,
                        request.NameEN,
                        request.Description,
                        GenerateClubCode(),
                        request.IsPublic,
                        request.LimitParticipant,
                        request.LimitClubManager,
                        request.RequesterID,
                        _clock.Now,
                        request.ImageUrl,
                        managerID: Guid.Empty,
                        droneID: request.DroneID,
                        clubPolicy: request.ClubPolicy
                    );

                    await _unitOfWork.Clubs.Add(newClub);
                    await _unitOfWork.SaveChangeAsync();

                    // Approve request with the created club's ID
                    request.Approve(approverId, newClub.ClubID, _clock.Now);
                    break;

                case ClubCreationRequestStatus.REJECTED:
                    if (!isAdmin && !isSystemManager)
                        throw new ForbiddenException("Chỉ SYSTEM_MANAGER hoặc ADMIN mới có quyền reject.");

                    if (string.IsNullOrWhiteSpace(dto.RejectReason))
                        throw new ArgumentException("Reject reason is required for rejection.");
                    request.Reject(approverId, dto.RejectReason, _clock.Now);
                    break;

                case ClubCreationRequestStatus.CANCEL:
                    if (!isRequester)
                        throw new ForbiddenException("Chỉ người tạo request mới được cancel.");

                    request.Cancel(request.RequesterID);
                    break;

                default:
                    throw new InvalidOperationException($"Cannot update to status {dto.Status}.");
            }

            // Save changes
            await _unitOfWork.ClubCreationRequests.Update(request);
            await _unitOfWork.SaveChangeAsync();

            // Return response
            var response = new ClubCreationRequestUpdateStatusResponseDto
            {
                ClubCreationRequestID = request.ClubCreationRequestID,
                NameVN = request.NameVN,
                NameEN = request.NameEN,
                Status = request.Status,
                UpdatedAt = request.UpdatedAt ?? DateTime.UtcNow,
                RejectReason = request.RejectReason,
                ClubID = request.ClubID
            };

            return response;
        }

        public async Task<ClubCreationRequestUpdateInfoResponseDto> UpdateRequestInfo(Guid id, ClubCreationRequestUpdateInfoDto dto)
        {
            var requesterId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

            // Get the request from database with categories
            var request = await _unitOfWork.ClubCreationRequests.GetByCondition(
                r => r.ClubCreationRequestID == id,
                query => query
            );

            if (request == null)
                throw new KeyNotFoundException($"Club creation request with ID {id} not found.");

            // Update basic information using domain method
            request.UpdateInfo(
                dto.NameVN,
                dto.NameEN,
                dto.Description,
                dto.IsPublic,
                dto.LimitParticipant,
                dto.LimitClubManager,
                dto.Image,
                requesterId
            );

            // Save changes
            await _unitOfWork.ClubCreationRequests.Update(request);
            await _unitOfWork.SaveChangeAsync();

            // Reload to get updated categories
            var updatedRequest = await _unitOfWork.ClubCreationRequests.GetByCondition(
                r => r.ClubCreationRequestID == id,
                query => query
            ) ?? throw new KeyNotFoundException($"Club creation request with ID {id} not found.");

            // Return response
            return new ClubCreationRequestUpdateInfoResponseDto
            {
                ClubCreationRequestID = updatedRequest.ClubCreationRequestID,
                NameVN = updatedRequest.NameVN,
                NameEN = updatedRequest.NameEN,
                Description = updatedRequest.Description,
                IsPublic = updatedRequest.IsPublic,
                LimitParticipant = updatedRequest.LimitParticipant,
                LimitClubManager = updatedRequest.LimitClubManager,
                ImageUrl = updatedRequest.ImageUrl,
                UpdatedAt = updatedRequest.UpdatedAt,
                Status = updatedRequest.Status
                
            };
        }

        /// <summary>
        /// Generate a unique 6-character club code
        /// </summary>
        private string GenerateClubCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string([.. Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)])]);
        }
    }
}
