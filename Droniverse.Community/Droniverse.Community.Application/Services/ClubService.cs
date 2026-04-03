using AutoMapper;
using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Application.Services;
internal class ClubService : IClubService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IdentityMicroserviceClient _identityMicroserviceClient;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public ClubService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IdentityMicroserviceClient identityMicroserviceClient,
        AcademyMicroserviceClient academyMicroserviceClient,
        ICurrentUserService currentUserService,
        IClock clock
        )
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _identityMicroserviceClient = identityMicroserviceClient;
        _academyMicroserviceClient = academyMicroserviceClient;
        _currentUserService = currentUserService;
        _clock = clock;
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

        Guid currentUserID = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));
        var user = await GetRequiredUserById(currentUserID);

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
        if (createdClub == null)
            throw new KeyNotFoundException($"Club with ID [{club.ClubID}] not found.");

        return await BuildClubResponseDto(createdClub, user);
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

    public async Task<PaginationResult<IEnumerable<ClubResponseDto>>> GetAllClubs(GetAllClubsSearchRequest request)
    {
        request ??= new GetAllClubsSearchRequest();

        var currentPage = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        var pageSize = request.PageSize < 5 ? 5 : (request.PageSize > 20 ? 20 : request.PageSize);

        var clubResult = await _unitOfWork.Clubs.GetAllWithCategories(
                   request.ClubName,
                   request.ClubStatus,
                   currentPage,
                   pageSize);

        var clubList = clubResult.Data?.ToList() ?? [];
        if (clubList.Count == 0)
            return new PaginationResult<IEnumerable<ClubResponseDto>>([], clubResult.TotalRecords, currentPage, pageSize);

        var mappedClubs = await BuildClubResponseDtos(clubList);

        return new PaginationResult<IEnumerable<ClubResponseDto>>(
            mappedClubs,
            clubResult.TotalRecords,
            currentPage,
            pageSize
        );
    }

    public async Task<ClubResponseDto> GetClubById(Guid id)
    {
        Club? club = await _unitOfWork.Clubs.GetByIdWithCategories(id);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with ID {id} not found.");
        }

        return await BuildClubResponseDto(club);
    }

    public async Task<ClubResponseDto> GetClubByClubCode(string clubCode)
    {
        Club? club = await _unitOfWork.Clubs.GetByClubCodeWithCategories(clubCode);
        if (club == null)
        {
            throw new KeyNotFoundException($"Club with club code [{clubCode}] not found.");
        }

        return await BuildClubResponseDto(club);
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
        if (updatedClub == null)
            throw new KeyNotFoundException($"Club with ID [{id}] not found.");

        return await BuildClubResponseDto(updatedClub);
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
        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID, query => query.AsNoTracking());
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

        if (ids.Count == 0)
            return [];

        try
        {
            return await _identityMicroserviceClient.GetUsersBulk(ids);
        }
        catch
        {
            Console.WriteLine("Không lấy được thông tin users khi gọi API");
            return [];
        }
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

    public async Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetClubCourses(Guid clubId, CourseBulkSearchRequest searchRequest)
    {
        Club? club = await _unitOfWork.Clubs.GetByCondition(
            c => c.ClubID == clubId,
            query => query.AsNoTracking());

        if (club == null)
            throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

        var clubCourses = await _unitOfWork.ClubCourses.GetManyByCondition(
            c => c.ClubID == clubId,
            query => query.AsNoTracking());

        var courseIds = clubCourses?
            .Select(c => c.CourseID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (courseIds.Count == 0)
            return Enumerable.Empty<CourseBulkResponseDTO>().ToPaginationResult(searchRequest);

        var course = await _academyMicroserviceClient.GetCourseById(courseIds, searchRequest);


        return course.ToPaginationResult(searchRequest);
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

        return await BuildClubResponseDtos(clubs);
    }

    private async Task<IEnumerable<ClubResponseDto>> BuildClubResponseDtos(IEnumerable<Club> clubs)
    {
        var clubList = clubs?.ToList() ?? [];
        if (clubList.Count == 0)
            return [];

        var userIds = clubList.Select(c => c.CreatedBy).Distinct().ToList();
        var users = await GetUsersByIds(userIds);
        var userDict = users.ToDictionary(u => u.UserId, u => u);

        var clubStats = await GetClubStatsByClubIds(clubList.Select(c => c.ClubID));

        return clubList.Select(club =>
        {
            userDict.TryGetValue(club.CreatedBy, out var creator);
            return BuildClubResponseDto(club, clubStats, creator);
        });
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
                club.Restore(_clock.Now);
                break;

            case Domain.Enums.ClubStatus.INACTIVE:
                club.Deactivate(_clock.Now);
                break;

            case Domain.Enums.ClubStatus.SUSPENDED:
                club.Suspend(_clock.Now);
                break;

            case Domain.Enums.ClubStatus.ARCHIVED:
                club.Archive(_clock.Now);
                break;

            default:
                throw new ArgumentException($"Invalid status: {dto.Status}");
        }

        await _unitOfWork.Clubs.Update(club);
        await _unitOfWork.SaveChangeAsync();

        return await BuildClubResponseDto(club);
    }

    private async Task<ClubResponseDto> BuildClubResponseDto(Club club, UserResponse? creator = null)
    {
        var clubStats = await GetClubStatsByClubIds([club.ClubID]);
        creator ??= await GetUserById(club.CreatedBy);

        return BuildClubResponseDto(club, clubStats, creator);
    }

    private ClubResponseDto BuildClubResponseDto(
        Club club,
        IReadOnlyDictionary<Guid, (int MemberCount, int CourseCount)> clubStats,
        UserResponse? creator)
    {
        var response = _mapper.Map<ClubResponseDto>(club);
        response.Creator = creator;

        var (memberCount, courseCount) = clubStats.GetValueOrDefault(club.ClubID, (0, 0));
        response.TotalMembers = memberCount;
        response.TotalCourses = courseCount;

        return response;
    }

    private async Task<Dictionary<Guid, (int MemberCount, int CourseCount)>> GetClubStatsByClubIds(IEnumerable<Guid> clubIds)
    {
        return await _unitOfWork.Clubs.GetClubStatsByClubIds(clubIds);
    }

    private async Task<UserResponse?> GetUserById(Guid userId)
    {
        return (await GetUsersByIds([userId])).FirstOrDefault();
    }

    private async Task<UserResponse> GetRequiredUserById(Guid userId)
    {
        var user = await GetUserById(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID [{userId}] not found.");

        return user;
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
                if (!isAdmin && !isSystemManager && !isClubManager && !isClubOwner)
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

