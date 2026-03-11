using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services;
internal class ClubService : IClubService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly ICurrentUserService _currentUserService;

    public ClubService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IdentityMicroserviceClient identityMicroserviceClient,
        AcademyMicroserviceClient academyMicroserviceClient,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _identityMicroserviceClient = identityMicroserviceClient;
        _academyMicroserviceClient = academyMicroserviceClient;
        _currentUserService = currentUserService;
    }

    public async Task<ClubResponseDto> CreateClub(ClubCreateDto clubRequestDto)
    {
        if (clubRequestDto == null)
        {
            throw new ArgumentNullException(nameof(clubRequestDto), "Club request data cannot be null.");
        }

        if (clubRequestDto.CategoryIDs != null && clubRequestDto.CategoryIDs.Any())
        {
            foreach (var categoryId in clubRequestDto.CategoryIDs)
            {
                var category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == categoryId);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
                }
            }
        }

        Club club = _mapper.Map<Club>(clubRequestDto);
        club.ClubID = Guid.NewGuid();
        club.ClubCode = GenerateClubCode();

        UserResponse user;

        Guid currentUserID = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));
        user = await _identityMicroserviceClient.GetUserByUserID(currentUserID);

        if (user == null)
            throw new KeyNotFoundException($"User with ID [{currentUserID}] not found.");

        club.CreatedBy = user.UserId;

        await _unitOfWork.Clubs.Add(club);
        await _unitOfWork.SaveChangeAsync();

        if (clubRequestDto.CategoryIDs != null && clubRequestDto.CategoryIDs.Any())
        {
            foreach (var categoryId in clubRequestDto.CategoryIDs)
            {
                var clubCategory = new ClubCategory
                {
                    ClubID = club.ClubID,
                    CategoryID = categoryId
                };
                await _unitOfWork.ClubCategories.Add(clubCategory);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        var createdClub = await _unitOfWork.Clubs.GetByIdWithCategories(club.ClubID);

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(createdClub);
        response = response with { Creator = user };
        return response;
    }

    private static string GenerateClubCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ123456789";
        var random = new Random();

        return new string(
            Enumerable.Range(0, length)
                      .Select(_ => chars[random.Next(chars.Length)])
                      .ToArray()
        );
    }

    public async Task<bool> DeleteClub(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        try
        {
            await _unitOfWork.Clubs.Delete(club);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<IEnumerable<ClubResponseDto>> GetAllClubs()
    {
        IEnumerable<Club> clubList = await _unitOfWork.Clubs.GetAllWithCategories();
        return await MapClubsWithStats(clubList);
    }

    public async Task<ClubResponseDto> GetClubById(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByIdWithCategories(id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }

        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds(new[] { id });
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds(new[] { id });

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        response.TotalMembers = memberCounts.GetValueOrDefault(id, 0);
        response.TotalCourses = courseCounts.GetValueOrDefault(id, 0);
        return response;
    }

    public async Task<ClubResponseDto> GetClubByClubCode(string clubCode)
    {
        Club? club = await _unitOfWork.Clubs.GetByClubCodeWithCategories(clubCode);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with club code [{clubCode}] not found.");
        }

        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds([club.ClubID]);
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds([club.ClubID]);

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        response.TotalMembers = memberCounts.GetValueOrDefault(club.ClubID, 0);
        response.TotalCourses = courseCounts.GetValueOrDefault(club.ClubID, 0);
        return response;
    }

    public async Task<ClubResponseDto> UpdateClub(Guid id, ClubUpdateDto clubUpdateDto)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }
        _mapper.Map(clubUpdateDto, club);
        await _unitOfWork.Clubs.Update(club);
        await _unitOfWork.SaveChangeAsync();

        var updatedClub = await _unitOfWork.Clubs.GetByIdWithCategories(id);
        ClubResponseDto response = _mapper.Map<ClubResponseDto>(updatedClub);
        return response;
    }

    public async Task<JoinClubResponse> JoinClub(ClubJoinDto request)
    {
        var club = await _unitOfWork.Clubs
            .GetByCondition(c => c.ClubCode == request.clubCode);

        if (club == null)
            throw new KeyNotFoundException($"Club with club code {request.clubCode} not found.");

        var currentUserId = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        bool isUserExisted = await _unitOfWork.Participations.IsUserInClub(club.ClubID, currentUserId);

        if (isUserExisted)
            throw new InvalidOperationException($"Thành viên này đã là thuộc câu lạc bộ [{club.NameVN}]");

        JoinClubResponse response = new()
        {
            ClubID = club.ClubID,
            NameEN = club.NameEN,
            NameVN = club.NameVN,
            ClubIsPublic = club.IsPublic,
        };

        if (club.IsPublic)
        {
            var participation = new Participation(
                currentUserId,
                club.ClubID,
                null
            );

            await _unitOfWork.Participations.Add(participation);
        }
        else
        {
            bool user = await _unitOfWork.ClubAttemptRequests.IsUserInClubAttemptRequest(currentUserId, club.ClubID);

            if (user == true)
                throw new InvalidOperationException("Yêu cầu tham gia club của người dùng này đang chờ được duyệt !");

            var clubAttemptRequest = new ClubAttemptRequest(
                currentUserId,
                club.ClubID
            );

            await _unitOfWork.ClubAttemptRequests.Add(clubAttemptRequest);
            response.ClubAttemptRequestID = clubAttemptRequest.ClubRequestID;
        }

        await _unitOfWork.SaveChangeAsync();

        return response;
    }

    public async Task<PaginationResult<IEnumerable<UserResponse>>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {clubID} not found.");
        }

        var participations = await _unitOfWork.Participations.GetManyByCondition(
            p => p.ClubID == clubID && p.Status == Domain.Enums.ParticipationStatus.ACTIVE
        );

        var userIds = participations
            .Select(p => p.UserID)
            .Distinct()
            .ToList();

        if (!userIds.Any())
        {
            return Enumerable.Empty<UserResponse>().ToPaginationResult(searchRequest);
        }

        var users = await GetUsersByIds(userIds);
        var filteredUsers = ApplyParticipationUserFilters(users, searchRequest);

        return filteredUsers.ToPaginationResult(searchRequest);
    }

    private async Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds)
    {
        var ids = userIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return Enumerable.Empty<UserResponse>();

        return await _identityMicroserviceClient.GetUsersBulk(ids);
    }

    private static IEnumerable<UserResponse> ApplyParticipationUserFilters(
        IEnumerable<UserResponse> users,
        ParticipationSearchRequest searchRequest)
    {
        IEnumerable<UserResponse> query = users;

        if (!string.IsNullOrWhiteSpace(searchRequest.ParicipationName))
        {
            var keyword = searchRequest.ParicipationName.Trim();
            query = query.Where(u =>
                ($"{u.FirstName} {u.LastName}").Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (searchRequest.DateOfBirth.HasValue)
        {
            query = query.Where(u =>
                u.DateOfBirth.HasValue
                && DateOnly.FromDateTime(u.DateOfBirth.Value.Date) == searchRequest.DateOfBirth.Value);
        }

        return query;
    }

    public async Task<IEnumerable<DTO.Response.CourseResponseDto>> GetClubCourses(Guid clubId, ClubCourseSearchRequest searchRequest)
    {
        // todo
        throw new Exception();
    }

    public async Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID(ClubStatus? status = null)
    {
        var currentUserId = Guid.Parse(_currentUserService.UserID 
            ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        var roles = _currentUserService.Roles.ToList();
        bool isMember = roles.Contains(Droniverse.Shared.Constants.Roles.ClubMember);
        
        IEnumerable<Club> clubs;
        
        if (isMember)
            // CLUB_MEMBER: Lấy clubs đã tham gia
            clubs = await _unitOfWork.Clubs.GetClubsByParticipantUserId(currentUserId, status);
        else
            // CLUB_MANAGER/ADMIN/SYSTEM_MANAGER: Lấy clubs đã tạo
            clubs = await _unitOfWork.Clubs.GetClubsByClubManagerID(currentUserId, status);
        
        return await MapClubsWithStats(clubs);
    }

    private async Task<IEnumerable<ClubResponseDto>> MapClubsWithStats(IEnumerable<Club> clubs)
    {
        var clubList = clubs?.ToList() ?? new List<Club>();
        if (!clubList.Any())
            return Enumerable.Empty<ClubResponseDto>();

        var clubIds = clubList.Select(c => c.ClubID).Distinct().ToList();
        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds(clubIds);
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds(clubIds);

        return clubList.Select(club =>
        {
            var response = _mapper.Map<ClubResponseDto>(club);
            response.TotalMembers = memberCounts.GetValueOrDefault(club.ClubID, 0);
            response.TotalCourses = courseCounts.GetValueOrDefault(club.ClubID, 0);
            return response;
        }).ToList();
    }

    // ===== Status Management Methods =====
    
    /// <summary>
    /// Update Club Status với phân quyền động
    /// </summary>
    public async Task<ClubResponseDto> UpdateClubStatus(Guid clubId, ClubUpdateStatusDto dto)
    {
        var club = await _unitOfWork.Clubs.GetByIdWithCategories(clubId);
        if (club == null)
            throw new KeyNotFoundException($"Club with ID {clubId} not found.");

        var currentUserId = Guid.Parse(_currentUserService.UserID 
            ?? throw new UnauthorizedAccessException("User is not authenticated."));
        
        var userRoles = _currentUserService.Roles.ToList();

        ValidateStatusChangePermission(dto.Status, userRoles, club, currentUserId);

        //if ((dto.Status == Domain.Enums.ClubStatus.SUSPENDED || dto.Status == Domain.Enums.ClubStatus.ARCHIVED) 
        //    && string.IsNullOrWhiteSpace(dto.Reason))
        //{
        //    throw new ArgumentException($"Reason is required when changing status to {dto.Status}");
        //}

        switch (dto.Status)
        {
            case Domain.Enums.ClubStatus.ACTIVE:
                club.Restore(); 
                break;
                
            case Domain.Enums.ClubStatus.INACTIVE:
                club.Deactivate();
                break;
                
            case Domain.Enums.ClubStatus.SUSPENDED:
                club.Suspend();
                break;
                
            case Domain.Enums.ClubStatus.ARCHIVED:
                club.Archive();
                break;
                
            default:
                throw new ArgumentException($"Invalid status: {dto.Status}");
        }

        await _unitOfWork.Clubs.Update(club);
        await _unitOfWork.SaveChangeAsync();

        // Return response with stats
        var memberCounts = await _unitOfWork.Clubs.GetMemberCountsByClubIds([clubId]);
        var courseCounts = await _unitOfWork.Clubs.GetCourseCountsByClubIds([clubId]);

        ClubResponseDto response = _mapper.Map<ClubResponseDto>(club);
        response.TotalMembers = memberCounts.GetValueOrDefault(clubId, 0);
        response.TotalCourses = courseCounts.GetValueOrDefault(clubId, 0);
        
        return response;
    }

    /// <summary>
    /// Validate quyền thay đổi status
    /// </summary>
    private void ValidateStatusChangePermission(
        Domain.Enums.ClubStatus targetStatus, 
        List<string> userRoles, 
        Club club,
        Guid currentUserId)
    {
        bool isAdmin = userRoles.Contains(Droniverse.Shared.Constants.Roles.Admin);
        bool isSystemManager = userRoles.Contains(Droniverse.Shared.Constants.Roles.SystemManager);
        bool isClubManager = userRoles.Contains(Droniverse.Shared.Constants.Roles.ClubManager);
        bool isClubOwner = club.CreatedBy == currentUserId;

        switch (targetStatus)
        {
            case Domain.Enums.ClubStatus.SUSPENDED:
                // Only ADMIN or SYSTEM_MANAGER can SUSPEND
                if (!isAdmin && !isSystemManager)
                    throw new Droniverse.Shared.Exceptions.ForbiddenException(
                        "Only ADMIN or SYSTEM_MANAGER can suspend a club.");
                break;

            case Domain.Enums.ClubStatus.INACTIVE:
                // Only CLUB_MANAGER (owner) can DEACTIVATE
                if (!isClubManager || !isClubOwner)
                    throw new Droniverse.Shared.Exceptions.ForbiddenException(
                        "Only CLUB_MANAGER (owner) can deactivate a club.");
                break;

            case Domain.Enums.ClubStatus.ARCHIVED:
                // ADMIN, SYSTEM_MANAGER, or CLUB_MANAGER (owner) can ARCHIVE
                if (!isAdmin && !isSystemManager && !(isClubManager && isClubOwner))
                    throw new Droniverse.Shared.Exceptions.ForbiddenException(
                        "Only ADMIN, SYSTEM_MANAGER, or CLUB_MANAGER (owner) can archive a club.");
                break;

            case Domain.Enums.ClubStatus.ACTIVE:
                // Only ADMIN or SYSTEM_MANAGER or CLUB_MANAGER can RESTORE to ACTIVE
                if (!isAdmin && !isSystemManager && isClubManager)
                    throw new Droniverse.Shared.Exceptions.ForbiddenException(
                        "Only ADMIN or SYSTEM_MANAGER or CLUB_MANAGER can restore a club to ACTIVE.");
                break;

            default:
                throw new ArgumentException($"Invalid target status: {targetStatus}");
        }
    }
    
    [Obsolete("Use UpdateClubStatus instead")]
    public async Task<ClubResponseDto> SuspendClub(Guid clubId, string? reason = null)
    {
        return await UpdateClubStatus(clubId, new ClubUpdateStatusDto 
        { 
            Status = Domain.Enums.ClubStatus.SUSPENDED, 
            Reason = reason 
        });
    }

    [Obsolete("Use UpdateClubStatus instead")]
    public async Task<ClubResponseDto> ArchiveClub(Guid clubId, string? reason = null)
    {
        return await UpdateClubStatus(clubId, new ClubUpdateStatusDto 
        { 
            Status = Domain.Enums.ClubStatus.ARCHIVED, 
            Reason = reason 
        });
    }

    [Obsolete("Use UpdateClubStatus instead")]
    public async Task<ClubResponseDto> RestoreClub(Guid clubId)
    {
        return await UpdateClubStatus(clubId, new ClubUpdateStatusDto 
        { 
            Status = Domain.Enums.ClubStatus.ACTIVE 
        });
    }

    [Obsolete("Use UpdateClubStatus instead")]
    public async Task<ClubResponseDto> DeactivateClub(Guid clubId)
    {
        return await UpdateClubStatus(clubId, new ClubUpdateStatusDto 
        { 
            Status = Domain.Enums.ClubStatus.INACTIVE 
        });
    }
}

