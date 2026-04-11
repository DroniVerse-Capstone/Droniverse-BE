using AutoMapper;
using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Academy.Application.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Droniverse.Shared.Constants;
using Droniverse.Shared.Enums;
using Droniverse.Shared.DTOs;

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

        var participationList = participations.ToList();
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
            var pageUsers = await GetUsersByIds(pageUserIds);
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
                    JoinDate = p.JoinDate
                };
            })
            .OrderByDescending(u => u.JoinDate)
            .ToList();

        return new PaginationResult<IEnumerable<GetParticipantsResponse>>(data, totalRecords, currentPage, pageSize);
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

    public async Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetClubCourses(
     Guid clubId,
     CourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new CourseBulkSearchRequest();

        // 1. Xác định role
        var isMember = _currentUserService.Roles.Contains(Roles.ClubMember);
        var effectiveOwnerFilter = isMember
            ? CourseOwnerFilter.Owned
            : searchRequest.CourseOwner;

        // 2. Check club tồn tại
        var clubExists = await _unitOfWork.Clubs.GetByCondition(c => c.ClubID == clubId);
        if (clubExists == null)
            throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

        // 3. Lấy danh sách course của club khi cần Owned/NotOwned
        List<ClubCourse> clubCourses = [];
        List<Guid> courseIds = [];

        if (effectiveOwnerFilter is CourseOwnerFilter.Owned or CourseOwnerFilter.NotOwned)
        {
            clubCourses = (await _unitOfWork.ClubCourses.GetManyByCondition(
                cc => cc.ClubID == clubId,
                q => q.AsNoTracking()))
                ?.ToList() ?? [];

            courseIds = clubCourses
                .Select(c => c.CourseID)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (effectiveOwnerFilter == CourseOwnerFilter.Owned && courseIds.Count == 0)
            {
                return Enumerable.Empty<CourseBulkResponseDTO>()
                    .ToPaginationResult(searchRequest);
            }
        }

        // 4. Build request cho academy
        var academyRequest = new CourseBulkSearchRequest
        {
            CurrentPage = searchRequest.CurrentPage,
            PageSize = searchRequest.PageSize,
            Level = searchRequest.Level,
            ParticipationSort = searchRequest.ParticipationSort,
            CourseName = searchRequest.CourseName,
            CourseOwner = effectiveOwnerFilter
        };

        // 5. Call academy
        var pagedCourse = await _academyMicroserviceClient.GetCourseById(
            effectiveOwnerFilter is CourseOwnerFilter.Owned or CourseOwnerFilter.NotOwned ? courseIds : [],
            academyRequest);

        var courseItems = pagedCourse.Items?.ToList() ?? [];

        if (courseItems.Count == 0)
        {
            return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
                courseItems,
                pagedCourse.TotalItems,
                searchRequest.CurrentPage,
                searchRequest.PageSize);
        }

        // 6. Lấy danh sách courseId trong page
        var itemCourseIds = courseItems
            .Select(c => c.CourseId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // 7. Query product (price)
        var products = await _unitOfWork.Products.GetManyByCondition(
            p => itemCourseIds.Contains(p.ReferenceID) && p.Status == ProductStatus.ACTIVE,
            q => q.AsNoTracking());

        var priceByCourseId = products?
            .GroupBy(p => p.ReferenceID)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(x => x.UpdateAt)
                    .Select(x => (decimal?)x.Price)
                    .FirstOrDefault()
            ) ?? [];

        // 8. Gộp remaining + profitType vào một map duy nhất
        Dictionary<Guid, (int Remaining, ClubCourseProfit ProfitType)> clubCourseMap = new();

        IEnumerable<ClubCourse> sourceClubCourses;

        if (clubCourses.Count > 0)
        {
            sourceClubCourses = clubCourses;
        }
        else
        {
            sourceClubCourses = await _unitOfWork.ClubCourses.GetManyByCondition(
                cc => cc.ClubID == clubId && itemCourseIds.Contains(cc.CourseID),
                q => q.AsNoTracking()) ?? [];
        }

        clubCourseMap = sourceClubCourses
            .Where(cc => itemCourseIds.Contains(cc.CourseID))
            .GroupBy(cc => cc.CourseID)
            .ToDictionary(
                g => g.Key,
                g => (
                    g.First().RemainingQuantity,
                    g.First().ProfitType
                )
            );

        // 9. Map dữ liệu
        foreach (var course in courseItems)
        {
            course.Price = priceByCourseId.TryGetValue(course.CourseId, out var price)
                ? price ?? 0
                : 0;

            if (clubCourseMap.TryGetValue(course.CourseId, out var clubData))
            {
                course.ClubCourseOwned = new ClubCourseOwnedResponse
                {
                    RemainingCode = clubData.Remaining,
                    ProfitType = clubData.ProfitType
                };
            }
            else
            {
                course.ClubCourseOwned = null;
            }
        }

        // 10. Return
        return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
            courseItems,
            pagedCourse.TotalItems,
            searchRequest.CurrentPage,
            searchRequest.PageSize);
    }

    /// <summary>
    /// Lấy danh sách khóa học hot của một câu lạc bộ theo phân trang.
    /// </summary>
    public async Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetHotCoursesByClub(Guid clubId, HotCoursesSearchRequest searchRequest)
    {
        searchRequest ??= new HotCoursesSearchRequest();

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
            return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>([], 0, searchRequest.CurrentPage, searchRequest.PageSize);

        var pagedHotCourse = await _academyMicroserviceClient.GetHotCoursesByIds(courseIds, searchRequest);

        var hotCourseItems = pagedHotCourse.Items?.ToList() ?? [];
        if (hotCourseItems.Count == 0)
        {
            return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
                hotCourseItems,
                pagedHotCourse.TotalItems,
                searchRequest.CurrentPage,
                searchRequest.PageSize);
        }

        var itemCourseIds = hotCourseItems
            .Select(c => c.CourseId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var products = await _unitOfWork.Products.GetManyByCondition(
            p => itemCourseIds.Contains(p.ReferenceID) && p.Status == ProductStatus.ACTIVE,
            q => q.AsNoTracking());

        var priceByCourseId = products?
            .GroupBy(p => p.ReferenceID)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(x => x.UpdateAt)
                    .Select(x => x.Price)
                    .FirstOrDefault()
            ) ?? [];

        var clubCourseMap = clubCourses
            .Where(cc => itemCourseIds.Contains(cc.CourseID))
            .GroupBy(cc => cc.CourseID)
            .ToDictionary(
                g => g.Key,
                g => new ClubCourseOwnedResponse
                {
                    RemainingCode = g.First().RemainingQuantity,
                    ProfitType = g.First().ProfitType
                });

        foreach (var course in hotCourseItems)
        {
            course.Price = priceByCourseId.TryGetValue(course.CourseId, out var price)
                ? price
                : 0;

            course.ClubCourseOwned = clubCourseMap.TryGetValue(course.CourseId, out var owned)
                ? owned
                : null;
        }

        return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(
            hotCourseItems,
            pagedHotCourse.TotalItems,
            searchRequest.CurrentPage,
            searchRequest.PageSize);
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

    public async Task<PaginationResult<IEnumerable<ManagerCoursesBulkResponseDTO>>> GetClubCoursesManagement(Guid clubId, ManagerCourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new ManagerCourseBulkSearchRequest();

        var club = await _unitOfWork.Clubs.GetByCondition(
            c => c.ClubID == clubId,
            q => q.AsNoTracking());

        if (club == null)
            throw new KeyNotFoundException($"Không tìm thấy câu lạc bộ với ID [{clubId}].");

        var clubCourses = (await _unitOfWork.ClubCourses.GetManyByCondition(
            cc => cc.ClubID == clubId,
            q => q.AsNoTracking()))?.ToList() ?? [];

        if (clubCourses.Count == 0)
            return new PaginationResult<IEnumerable<ManagerCoursesBulkResponseDTO>>([], 0, searchRequest.CurrentPage, searchRequest.PageSize);

        if (searchRequest.ProfitType.HasValue)
        {
            clubCourses = clubCourses
                .Where(cc => cc.ProfitType == searchRequest.ProfitType.Value)
                .ToList();
        }

        var courseIds = clubCourses
            .Select(cc => cc.CourseID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (courseIds.Count == 0)
            return new PaginationResult<IEnumerable<ManagerCoursesBulkResponseDTO>>([], 0, searchRequest.CurrentPage, searchRequest.PageSize);

        var pagedCourses = await _academyMicroserviceClient.GetCoursesByIdsManagement(courseIds, searchRequest);
        var academyItems = pagedCourses.Items?.ToList() ?? [];

        if (academyItems.Count == 0)
            return new PaginationResult<IEnumerable<ManagerCoursesBulkResponseDTO>>([], pagedCourses.TotalItems, searchRequest.CurrentPage, searchRequest.PageSize);

        var clubCourseByCourseId = clubCourses
            .GroupBy(cc => cc.CourseID)
            .ToDictionary(g => g.Key, g => g.First());

        var itemCourseIds = academyItems
            .Select(c => c.CourseId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var products = await _unitOfWork.Products.GetManyByCondition(
            p => itemCourseIds.Contains(p.ReferenceID) && p.Status == ProductStatus.ACTIVE,
            q => q.AsNoTracking());

        var priceByCourseId = products?
            .GroupBy(p => p.ReferenceID)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdateAt)
                      .Select(x => (decimal?)x.Price)
                      .FirstOrDefault())
            ?? [];

        var result = academyItems
            .Select(c =>
            {
                clubCourseByCourseId.TryGetValue(c.CourseId, out var clubCourse);

                return new ManagerCoursesBulkResponseDTO
                {
                    CourseId = c.CourseId,
                    CourseVersionId = c.CourseVersionId,
                    TitleVN = c.TitleVN,
                    TitleEN = c.TitleEN,
                    ImageUrl = c.ImageUrl ?? string.Empty,
                    Level = c.Level,
                    EstimatedDuration = c.EstimatedDuration,
                    NumberOfParticipants = c.NumberOfParticipants,
                    Price = priceByCourseId.TryGetValue(c.CourseId, out var price) ? price : null,
                    ClubCourseInfo = clubCourse == null
                        ? null
                        : new ManagerClubCourseOwnedResponse
                        {
                            TotalCode = clubCourse.TotalQuantity,
                            RemainingCode = clubCourse.RemainingQuantity,
                            ProfitType = clubCourse.ProfitType
                        }
                };
            })
            .ToList();

        if (searchRequest.CourseSortBy == ManagerCourseSortBy.Total_Codes_Quantity)
        {
            result = searchRequest.CourseSortDirection == SortDirection.Desc
                ? result.OrderByDescending(x =>
                    clubCourseByCourseId.TryGetValue(x.CourseId, out var cc) ? cc.TotalQuantity : 0).ToList()
                : result.OrderBy(x =>
                    clubCourseByCourseId.TryGetValue(x.CourseId, out var cc) ? cc.TotalQuantity : 0).ToList();
        }
        else if (searchRequest.CourseSortBy == ManagerCourseSortBy.Remaining_Codes_Quantity)
        {
            result = searchRequest.CourseSortDirection == SortDirection.Desc
                ? result.OrderByDescending(x => x.ClubCourseInfo?.RemainingCode ?? 0).ToList()
                : result.OrderBy(x => x.ClubCourseInfo?.RemainingCode ?? 0).ToList();
        }

        return new PaginationResult<IEnumerable<ManagerCoursesBulkResponseDTO>>(
            result,
            pagedCourses.TotalItems,
            searchRequest.CurrentPage,
            searchRequest.PageSize);
    }
}

