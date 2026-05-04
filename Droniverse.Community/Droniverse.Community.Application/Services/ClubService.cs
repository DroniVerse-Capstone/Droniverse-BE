using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
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

        Club club = _mapper.Map<Club>(clubRequestDto);
        club.ClubID = Guid.NewGuid();
        club.ClubCode = GenerateClubCode();

        Guid currentUserID = Guid.Parse(_currentUserService.UserID ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));
        var user = await GetRequiredUserById(currentUserID);

        club.CreatedBy = user.UserId;

        await _unitOfWork.Clubs.Add(club);

        return await BuildClubResponseDto(club, user);
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

        var clubResult = await _unitOfWork.Clubs.GetAllWithPolicies(
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
        Guid userID = _currentUserService.UserId;
        if (userID == Guid.Empty)
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");

        UserResponse? user = await _identityMicroserviceClient.GetUserByUserID(userID);
        if (user == null)
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");

        var userRoles = _currentUserService.Roles;
        var isMember = userRoles.Contains(Roles.ClubMember);

        if (isMember)
        {
            Participation? participation = await _unitOfWork.Participations.GetByCondition(p =>
                p.ClubID == id &&
                p.UserID == userID &&
                p.Status == ParticipationStatus.ACTIVE);
            if (participation == null)
            {
                throw new ForbiddenException("Bạn đã rời câu lạc bộ này, vui lòng liên hệ quản lý câu lạc bộ (club manager) hoặc admin để biết thêm chi tiết.");
            }
        }

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

        var creator = await GetUserById(club.CreatedBy);
        var drone = await GetDroneById(club.DroneID);
        var clubStats = await GetClubStatsByClubIds([club.ClubID]);

        return BuildClubResponseDto(club, clubStats, creator, drone);
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

        Guid? mediaId = null;
        if (request.mediaID.HasValue)
        {
            Media? media = await _unitOfWork.Medias.GetByCondition(m => m.MediaID == request.mediaID.Value);
            if (media == null)
                throw new KeyNotFoundException($"Không tìm thấy media với ID {request.mediaID}.");

            mediaId = media.MediaID;
        }

        JoinClubResponse response = new()
        {
            ClubID = club.ClubID,
            NameEN = club.NameEN,
            NameVN = club.NameVN
        };
        bool user = await _unitOfWork.ClubAttemptRequests.IsUserInClubAttemptRequest(currentUserId, club.ClubID);

        if (user == true)
            throw new InvalidOperationException("Yêu cầu tham gia club của người dùng này đang chờ được duyệt !");

        var clubAttemptRequest = new ClubAttemptRequest(
            currentUserId,
            club.ClubID,
            mediaId,
            request.clubRequirement
        );

        await _unitOfWork.ClubAttemptRequests.Add(clubAttemptRequest);
        response.ClubAttemptRequestID = clubAttemptRequest.ClubRequestID;

        await _unitOfWork.SaveChangeAsync();

        return response;
    }

    public async Task<bool> LeaveClub(Guid clubId)
    {
        var currentUserId = Guid.Parse(_currentUserService.UserID
            ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        var participation = await _unitOfWork.Participations.GetByCondition(
            p => p.ClubID == clubId && p.UserID == currentUserId && p.Status == ParticipationStatus.ACTIVE);

        if (participation == null)
            throw new KeyNotFoundException("Bạn không phải là thành viên đang hoạt động của câu lạc bộ này.");

        participation.Leave("Người dùng chủ động rời khỏi câu lạc bộ.");

        await _unitOfWork.Participations.Update(participation);
        await _unitOfWork.SaveChangeAsync();

        return true;
    }

    public async Task<KickMemberFromClubResponse> KickMemberFromClub(Guid clubId, Guid userId, ClubKickMemberRequest request)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không hợp lệ.");

        if (request == null)
            throw new ArgumentNullException(nameof(request), "Thông tin kick thành viên không được để trống.");

        var reason = request.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Lý do kick không được để trống.");

        var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId);
        if (club == null)
            throw new KeyNotFoundException($"Club with ID {clubId} not found.");

        var currentUserId = Guid.Parse(_currentUserService.UserID
            ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        var userRoles = _currentUserService.Roles.ToList();
        ValidateKickMemberPermission(club, userRoles, currentUserId, userId);

        var participation = await _unitOfWork.Participations.GetByCondition(
            p => p.ClubID == clubId && p.UserID == userId && p.Status == ParticipationStatus.ACTIVE);

        if (participation == null)
            throw new KeyNotFoundException("Người dùng này không phải là thành viên đang hoạt động của câu lạc bộ.");

        participation.Ban(reason);

        await _unitOfWork.Participations.Update(participation);
        await _unitOfWork.SaveChangeAsync();

        return new KickMemberFromClubResponse { ClubName = club.NameVN, Username = _currentUserService.UserName != null ? _currentUserService.UserName : "Thành viên" };
    }

    public async Task<IEnumerable<SimpleClubResponse>> GetClubInfoBulk(GetClubSimpleInfoRequest request)
    {

        var distinctIds = request.ClubIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinctIds.Count == 0)
            return [];

        var clubs = await _unitOfWork.Clubs
            .GetManyByConditionAsQueryable(c => distinctIds.Contains(c.ClubID), q => q.AsNoTracking())
            .Select(c => new SimpleClubResponse
            {
                ClubId = c.ClubID,
                ClubNameVN = c.NameVN,
                ClubNameEN = c.NameEN,
                ImageUrl = c.ImageUrl!,
                ClubStatus = c.Status
            })
            .ToListAsync();

        var clubById = clubs.ToDictionary(c => c.ClubId, c => c);

        return distinctIds
            .Where(clubById.ContainsKey)
            .Select(id => clubById[id])
            .ToList();
    }

    public async Task<PaginationResult<IEnumerable<GetParticipantsResponse>>> GetClubParcitipations(Guid clubID, ParticipationSearchRequest searchRequest)
    {
        searchRequest ??= new ParticipationSearchRequest();

        Club? club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubID, query => query.AsNoTracking());
        if (club == null)
            throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID {clubID}.");

        int currentPage = searchRequest.CurrentPage <= 0 ? 1 : searchRequest.CurrentPage;
        int pageSize = searchRequest.PageSize <= 0 ? 5 : searchRequest.PageSize;
        int skip = (currentPage - 1) * pageSize;

        var hasUserFilters = HasParticipationUserFilters(searchRequest);

        IEnumerable<Guid>? filteredUserIds = null;
        Dictionary<Guid, UserResponse>? preFilteredUserMap = null;

        if (hasUserFilters)
        {
            var activeUserIds = await _unitOfWork.Participations.GetActiveParticipantUserIdsByClubAsync(clubID);
            if (activeUserIds.Count == 0)
                return new PaginationResult<IEnumerable<GetParticipantsResponse>>([], 0, currentPage, pageSize);

            var users = (await GetUsersByIds(activeUserIds)).ToList();
            var filteredUsers = ApplyParticipationUserFilters(users, searchRequest).ToList();

            if (filteredUsers.Count == 0)
                return new PaginationResult<IEnumerable<GetParticipantsResponse>>([], 0, currentPage, pageSize);

            filteredUserIds = filteredUsers
                .Select(x => x.UserId)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            preFilteredUserMap = filteredUsers.ToDictionary(x => x.UserId, x => x);
        }

        var (totalRecords, participations) = await _unitOfWork.Participations.GetActiveParticipationsByClubAsync(
            clubID,
            skip,
            pageSize,
            filteredUserIds);

        if (totalRecords == 0)
            return new PaginationResult<IEnumerable<GetParticipantsResponse>>([], 0, currentPage, pageSize);

        List<Participation> participationList = participations.ToList();
        var pageUserIds = participationList
            .Select(p => p.UserID)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        Dictionary<Guid, UserResponse> userMap;
        if (preFilteredUserMap != null)
        {
            userMap = preFilteredUserMap;
        }
        else
        {
            IEnumerable<UserResponse> pageUsers = await GetUsersByIds(pageUserIds);
            userMap = pageUsers.ToDictionary(x => x.UserId, x => x);
        }

        var data = participationList
            .Select(p =>
            {
                userMap.TryGetValue(p.UserID, out var user);

                user ??= new UserResponse
                {
                    UserId = p.UserID,
                    Username = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = string.Empty,
                    RoleName = string.Empty,
                    ImageUrl = string.Empty
                };

                return new GetParticipantsResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    ImageUrl = user.ImageUrl,
                    Gender = user.Gender,
                    JoinDate = p.JoinDate,
                    UserLevel = user.UserLevel,
                    UserLevelMax = user.UserLevelMax
                };
            })
            .OrderByDescending(u => u.JoinDate)
            .ToList();

        return new PaginationResult<IEnumerable<GetParticipantsResponse>>(data, totalRecords, currentPage, pageSize);
    }

    private async Task<IEnumerable<DroneResponseDto>> GetDronesByIds(IEnumerable<Guid> droneIds)
    {
        var ids = droneIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
            return [];
        try
        {
            return await _academyMicroserviceClient.GetDronesBulk(ids);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Không lấy được thông tin drones khi gọi API. Error: {ex.Message}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return [];
        }
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
        catch (Exception ex)
        {
            Console.WriteLine($"Không lấy được thông tin users khi gọi API. Error: {ex.Message}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return [];
        }
    }

    private static IEnumerable<UserResponse> ApplyParticipationUserFilters(
        IEnumerable<UserResponse> users,
        ParticipationSearchRequest searchRequest)
    {
        IEnumerable<UserResponse> query = users;

        if (!string.IsNullOrWhiteSpace(searchRequest.ParticipantName))
        {
            var keyword = searchRequest.ParticipantName.Trim();
            query = query.Where(u => IsParticipationNameMatch(u, keyword));
        }

        if (searchRequest.DateOfBirth.HasValue)
        {
            query = query.Where(u =>
                u.DateOfBirth.HasValue
                && DateOnly.FromDateTime(u.DateOfBirth.Value.Date) == searchRequest.DateOfBirth.Value);
        }

        return query;
    }

    private static bool HasParticipationUserFilters(ParticipationSearchRequest searchRequest)
    {
        return !string.IsNullOrWhiteSpace(searchRequest.ParticipantName)
               || searchRequest.DateOfBirth.HasValue;
    }

    private static bool IsParticipationNameMatch(UserResponse user, string keyword)
    {
        var normalizedKeyword = keyword.Trim();
        if (string.IsNullOrWhiteSpace(normalizedKeyword))
            return true;

        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        if (fullName.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
            || user.FirstName.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
            || user.LastName.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
            || user.Username.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)
            || user.Email.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var tokens = normalizedKeyword
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
            return true;

        return tokens.All(token =>
            fullName.Contains(token, StringComparison.OrdinalIgnoreCase)
            || user.Username.Contains(token, StringComparison.OrdinalIgnoreCase)
            || user.Email.Contains(token, StringComparison.OrdinalIgnoreCase));
    }



    /// <summary>
    /// Lấy danh sách khóa học hot của một câu lạc bộ theo phân trang.
    /// </summary>
    //public async Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetHotCoursesByClub(Guid clubId, HotCoursesSearchRequest searchRequest)
    //{
    //    searchRequest ??= new HotCoursesSearchRequest();

    //    Club? club = await _unitOfWork.Clubs.GetByCondition(
    //        c => c.ClubID == clubId,
    //        query => query.AsNoTracking());

    //    if (club == null)
    //        throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");
    //    IEnumerable<Course>

    //    var courseIds = clubCourses
    //        .Where(id => id != Guid.Empty)
    //        .Distinct()
    //        .ToList() ?? [];

    //    if (courseIds.Count == 0)
    //        return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>([], 0, searchRequest.CurrentPage, searchRequest.PageSize);

    //    var pagedHotCourse = await _academyMicroserviceClient.GetHotCoursesByIds(courseIds, searchRequest);

    //    var hotCourseItems = pagedHotCourse.Items?.ToList() ?? [];
    //    if (hotCourseItems.Count == 0)
    //    {
    //        return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
    //            hotCourseItems,
    //            pagedHotCourse.TotalItems,
    //            searchRequest.CurrentPage,
    //            searchRequest.PageSize);
    //    }

    //    var itemCourseIds = hotCourseItems
    //        .Select(c => c.CourseId)
    //        .Where(id => id != Guid.Empty)
    //        .Distinct()
    //        .ToList();

    //    var products = await _unitOfWork.Products.GetManyByCondition(
    //        p => itemCourseIds.Contains(p.ReferenceID) && p.Status == ProductStatus.ACTIVE,
    //        q => q.AsNoTracking());

    //    var priceByCourseId = products?
    //        .GroupBy(p => p.ReferenceID)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g
    //                .OrderByDescending(x => x.UpdateAt)
    //                .Select(x => x.Price)
    //                .FirstOrDefault()
    //        ) ?? [];

    //    var clubCourseMap = clubCourses
    //        .Where(cc => itemCourseIds.Contains(cc.CourseID))
    //        .GroupBy(cc => cc.CourseID)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => new ClubCourseOwnedResponse
    //            {
    //                RemainingCode = g.First().RemainingQuantity,
    //                ProfitType = g.First().ProfitType
    //            });

    //    foreach (var course in hotCourseItems)
    //    {
    //        course.Price = priceByCourseId.TryGetValue(course.CourseId, out var price)
    //            ? price
    //            : 0;

    //        course.ClubCourseOwned = clubCourseMap.TryGetValue(course.CourseId, out var owned)
    //            ? owned
    //            : null;
    //    }

    //    return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
    //        hotCourseItems,
    //        pagedHotCourse.TotalItems,
    //        searchRequest.CurrentPage,
    //        searchRequest.PageSize);
    //}

    public async Task<IEnumerable<ClubResponseDto>> GetClubsByCurrentUsersID(ClubStatus? status = null)
    {
        var currentUserId = Guid.Parse(_currentUserService.UserID
            ?? throw new UnauthorizedAccessException("Người dùng chưa được xác thực."));

        var roles = _currentUserService.Roles.ToList();
        bool isMember = roles.Contains(Droniverse.Shared.Constants.Roles.ClubMember);

        IEnumerable<Club> clubs;

        if (isMember)
        {
            // CLUB_MEMBER: Lấy clubs đã tham gia
            // Check xem member đó còn trong group hay không
            Participation? participation = await _unitOfWork.Participations.GetByCondition(p =>
                p.UserID == currentUserId &&
                p.Status == ParticipationStatus.ACTIVE);
            if(participation == null)
            {
                throw new ForbiddenException("Bạn hiện tại đã không hoạt động trong câu lạc bộ này, vui lòng liên hệ quản lý câu lạc bộ (club manager) hoặc admin để biết thêm chi tiết.");
            }

            clubs = await _unitOfWork.Clubs.GetClubsByParticipantUserId(currentUserId, status);
        }
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

        var droneIds = clubList.Select(c => c.DroneID).Where(id => id != Guid.Empty).Distinct().ToList();
        var drones = await GetDronesByIds(droneIds);
        var droneDict = drones.ToDictionary(d => d.DroneID, d => d);

        var clubStats = await GetClubStatsByClubIds(clubList.Select(c => c.ClubID));

        return clubList.Select(club =>
        {
            userDict.TryGetValue(club.CreatedBy, out var creator);
            droneDict.TryGetValue(club.DroneID, out var drone);
            return BuildClubResponseDto(club, clubStats, creator, drone);
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
                if (string.IsNullOrWhiteSpace(dto.Reason))
                    throw new ArgumentException("Lý do đình chỉ không được để trống.");

                club.Suspend(_clock.Now, dto.Reason.Trim());
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

    private async Task<ClubResponseDto> BuildClubResponseDto(Club club, UserResponse? creator = null, DroneResponseDto? drone = null)
    {
        var clubStats = await GetClubStatsByClubIds([club.ClubID]);
        creator ??= await GetUserById(club.CreatedBy);
        drone ??= await GetDroneById(club.DroneID);

        return BuildClubResponseDto(club, clubStats, creator, drone);
    }

    private ClubResponseDto BuildClubResponseDto(
        Club club,
        IReadOnlyDictionary<Guid, (int MemberCount, int CourseCount)> clubStats,
        UserResponse? creator,
        DroneResponseDto? drone)
    {
        var response = _mapper.Map<ClubResponseDto>(club);
        response.Creator = creator;
        response.Drone = drone;

        var (memberCount, courseCount) = clubStats.GetValueOrDefault(club.ClubID, (0, 0));
        response.TotalMembers = memberCount;
        response.TotalCourses = courseCount;

        return response;
    }

    private async Task<Dictionary<Guid, (int MemberCount, int CourseCount)>> GetClubStatsByClubIds(IEnumerable<Guid> clubIds)
    {
        return await _unitOfWork.Clubs.GetClubStatsByClubIds(clubIds);
    }

    private async Task<DroneResponseDto?> GetDroneById(Guid droneId)
    {
        if (droneId == Guid.Empty)
            return null;

        return (await GetDronesByIds([droneId])).FirstOrDefault();
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
                if (!(isClubManager && isClubOwner))
                    throw new Droniverse.Shared.Exceptions.ForbiddenException(
                        "Only CLUB_MANAGER (owner) can archive a club.");
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

    private void ValidateKickMemberPermission(
        Club club,
        List<string> userRoles,
        Guid currentUserId,
        Guid targetUserId)
    {
        if (currentUserId == targetUserId)
            throw new InvalidOperationException("Không thể kick chính mình. Hãy sử dụng API rời khỏi câu lạc bộ.");

        bool isAdmin = userRoles.Contains(Droniverse.Shared.Constants.Roles.Admin);
        bool isSystemManager = userRoles.Contains(Droniverse.Shared.Constants.Roles.SystemManager);
        bool isClubManager = userRoles.Contains(Droniverse.Shared.Constants.Roles.ClubManager);
        bool isClubOwner = club.CreatedBy == currentUserId;

        if (isAdmin || isSystemManager)
            return;

        if (isClubManager && isClubOwner)
            return;

        throw new Droniverse.Shared.Exceptions.ForbiddenException(
            "Only CLUB_MANAGER (owner), ADMIN or SYSTEM_MANAGER can kick a club member.");
    }


    public async Task<GetClubParticipantsResponse> GetClubParticipantIds(Guid clubId, GetClubParticipantIdsRequest request)
    {
        SimpleClubResponse? club = await _unitOfWork.Clubs.GetSimpleClubInfoById(clubId);

        if (club == null)
            throw new KeyNotFoundException("Không tìm thấy câu lạc bộ");

        ValidateClubIsActive(club);

        var participantIds = await _unitOfWork.Participations.GetParicipantIdsByClubId(clubId, request.ParticipantStatus);

        return new GetClubParticipantsResponse { participantIds = participantIds ?? [] };
    }

    public async Task<bool> CheckParticipant(Guid clubId, Guid userId, ParticipationStatus status = ParticipationStatus.ACTIVE)
    {
        if (clubId == Guid.Empty)
            throw new ArgumentException("ClubId không hợp lệ.");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId không hợp lệ.");

        var clubExists = await _unitOfWork.Clubs
            .GetManyByConditionAsQueryable(c => c.ClubID == clubId, q => q.AsNoTracking())
            .AnyAsync();

        if (!clubExists)
            throw new KeyNotFoundException("Không tìm thấy câu lạc bộ");

        return await _unitOfWork.Participations
            .GetManyByConditionAsQueryable(
                p => p.ClubID == clubId && p.UserID == userId && p.Status == status,
                q => q.AsNoTracking())
            .AnyAsync();
    }

    private static void ValidateClubIsActive(SimpleClubResponse club)
    {
        if (club.ClubStatus != ClubStatus.ACTIVE)
        {
            throw new InvalidOperationException(
                $"Câu lạc bộ đang ở trạng thái {club.ClubStatus}, không thể thực hiện thao tác này.");
        }
    }

    public async Task<Guid> GetDroneFromClub(Guid clubId)
    {
        var club = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId);

        if (club == null)
            //throw new NotFoundException($"Club with ID {clubId} not found.");
            return Guid.Empty;

        return club.DroneID;
    }
}

