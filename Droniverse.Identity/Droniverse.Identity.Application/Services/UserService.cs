using AutoMapper;
using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.HttpClients;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Helpers;
using Droniverse.Shared.Messages.User;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Droniverse.Identity.Application.Services;

internal class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserPublisher _publisher;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly AcademyMicroserviceClient _academyMicroserviceClient;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserPublisher publisher,
        ICloudinaryService cloudinaryService,
        AcademyMicroserviceClient academyMicroserviceClient,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publisher = publisher;
        _cloudinaryService = cloudinaryService;
        _academyMicroserviceClient = academyMicroserviceClient;
        _logger = logger;
    }

    public async Task<UserDashboardSummaryResponse> GetUserDashboardSummary(string filterTimeLine = "month")
    {
        var (startDate, endDate) = GetTimelineRange(filterTimeLine);

        var totalUser = await _unitOfWork.Accounts.CountAsync();
        var newUsers = await _unitOfWork.Accounts.CountByCondition(a => a.CreateAt >= startDate && a.CreateAt < endDate);
        var memberCount = await _unitOfWork.Accounts.CountByCondition(a => a.Role.RoleName == RoleNameEnum.CLUB_MEMBER.ToString() && a.CreateAt >= startDate && a.CreateAt < endDate);
        var clubOwnerCount = await _unitOfWork.Accounts.CountByCondition(a => a.Role.RoleName == RoleNameEnum.CLUB_MANAGER.ToString() && a.CreateAt >= startDate && a.CreateAt < endDate);
        return new UserDashboardSummaryResponse
        {
            TotalUser = totalUser,
            NewUsers = newUsers,
            MemberCount = memberCount,
            ClubOwnerCount = clubOwnerCount
        };
    }

    private static (DateTime StartDate, DateTime EndDate) GetTimelineRange(string filterTimeLine)
    {
        if (string.IsNullOrWhiteSpace(filterTimeLine))
            filterTimeLine = "month";

        var now = DateTime.UtcNow;
        var normalized = filterTimeLine.Trim().ToLowerInvariant();

        if (normalized.StartsWith("month:", StringComparison.Ordinal))
        {
            return GetSpecificMonthRange(normalized[6..]);
        }

        if (normalized.StartsWith("year:", StringComparison.Ordinal))
        {
            return GetSpecificYearRange(normalized[5..]);
        }

        return normalized switch
        {
            "day" => (now.Date, now.Date.AddDays(1)),
            "week" => GetCurrentWeekRange(now),
            "last_week" => GetPreviousWeekRange(now),
            "month" => (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
            "last_month" => GetPreviousMonthRange(now),
            "year" => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddYears(1)),
            _ => throw new ArgumentException("filterTimeLine must be one of: day, week, last_week, month, last_month, year, month:YYYY-MM, year:YYYY.", nameof(filterTimeLine))
        };
    }

    public Task<IEnumerable<UserTimelineOptionResponse>> GetFilterTimeLineOptions()
    {
        // Options moved to Community Swagger example. Identity service no longer exposes hardcoded timeline examples.
        return Task.FromResult(Enumerable.Empty<UserTimelineOptionResponse>() as IEnumerable<UserTimelineOptionResponse>);
    }

    private static (DateTime StartDate, DateTime EndDate) GetCurrentWeekRange(DateTime now)
    {
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var startDate = now.Date.AddDays(-daysFromMonday);
        var endDate = startDate.AddDays(7);
        return (startDate, endDate);
    }

    private static (DateTime StartDate, DateTime EndDate) GetPreviousWeekRange(DateTime now)
    {
        var (currentWeekStart, _) = GetCurrentWeekRange(now);
        var previousWeekStart = currentWeekStart.AddDays(-7);
        var previousWeekEnd = currentWeekStart;
        return (previousWeekStart, previousWeekEnd);
    }

    private static (DateTime StartDate, DateTime EndDate) GetPreviousMonthRange(DateTime now)
    {
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        return (previousMonthStart, currentMonthStart);
    }

    private static (DateTime StartDate, DateTime EndDate) GetSpecificMonthRange(string monthValue)
    {
        if (!DateTime.TryParseExact(monthValue, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var parsedMonth))
        {
            throw new ArgumentException("month filter must use format month:YYYY-MM, for example month:2026-03.");
        }

        var startDate = new DateTime(parsedMonth.Year, parsedMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);
        return (startDate, endDate);
    }

    private static (DateTime StartDate, DateTime EndDate) GetSpecificYearRange(string yearValue)
    {
        if (!int.TryParse(yearValue, out var year) || year < 1 || year > 9999)
        {
            throw new ArgumentException("year filter must use format year:YYYY, for example year:2026.");
        }

        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddYears(1);
        return (startDate, endDate);
    }

    public async Task<IEnumerable<UserResponse>> GetNewUsersByTimeline(string filterTimeLine)
    {
        var (startDate, endDate) = GetTimelineRange(filterTimeLine);

        IEnumerable<Account> newAccounts = await _unitOfWork.Accounts.GetManyByCondition(a => a.CreateAt >= startDate && a.CreateAt < endDate);
        return _mapper.Map<IEnumerable<UserResponse>>(newAccounts);
    }

    public async Task<PaginationResult<IEnumerable<UserResponse>>> GetAllUsers(
        UserSearchRequest userSearchRequest,
        int pageIndex,
        int pageSize)
    {
        // Ưu tiên dùng CurrentPage/PageSize từ request nếu có
        var finalPageIndex = userSearchRequest?.CurrentPage ?? pageIndex;
        var finalPageSize = userSearchRequest?.PageSize ?? pageSize;

        return await _unitOfWork.Accounts.GetAllUsersAsync(
            userSearchRequest,
            finalPageIndex,
            finalPageSize);
    }

    public async Task<UserResponse> AddUser(UserCreateDto userCreateDto)
    {
        Role? role = await _unitOfWork.Roles.GetByCondition(r => r.RoleID == userCreateDto.RoleId);
        if (role == null)
        {
            throw new ArgumentException("Invalid role");
        }
        Account newAccount = new Account
        {
            Username = userCreateDto.Username,
            PasswordHash = userCreateDto.PasswordHash,
            Email = userCreateDto.Email,
            CreateAt = DateTime.UtcNow,
            RoleID = userCreateDto.RoleId

        };
        Account createdAccount = await _unitOfWork.Accounts.Add(newAccount);
        UserInfo newUserInfo = new UserInfo
        {
            UserID = createdAccount.UserID,
            FirstName = userCreateDto.FirstName,
            LastName = userCreateDto.LastName,
            DateOfBirth = userCreateDto.DateOfBirth,

        };

        UserInfo createdUserInfo = await _unitOfWork.UserInfos.Add(newUserInfo);
        await _unitOfWork.SaveChangeAsync();

        UserResponse userResponse = _mapper.Map<UserResponse>(createdAccount);
        return userResponse;
    }

    public async Task<UserResponse> GetUserById(Guid id)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == id);
        if (account == null)
        {
            throw new ArgumentException($"Account id not found #{id}");
        }
        UserInfo? userInfo = await _unitOfWork.UserInfos.GetByCondition(a => a.UserID == id);
        if (userInfo == null)
        {
            throw new ArgumentException($"User info id not found #{id}");
        }

        // Try to get user level from Academy service, but don't fail if unavailable
        IEnumerable<UserLevelResponseDto>? userLevel = null;
        IEnumerable<UserLevelResponseDto>? userLevelMax = null;
        try
        {
            userLevel = await _academyMicroserviceClient.GetUserLevelsAsync(id);
            userLevelMax = await _academyMicroserviceClient.GetUserLevelMaxAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to get user level from Academy service for user {id}: {ex.Message}");
            // Continue without level if Academy service is unavailable
        }

        UserResponse userResponse = _mapper.Map<UserResponse>(account);
        return userResponse with { UserLevel = userLevel, UserLevelMax = userLevelMax };
    }

    public async Task<UserResponse> UpdateUser(Guid userId, UserUpdateDto userUpdateDto)
    {
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == userId);
        if (account == null)
        {
            throw new ArgumentException($"Account id not found #{userId}");
        }
        UserInfo? userInfo = await _unitOfWork.UserInfos.GetByCondition(a => a.UserID == userId);
        if (userInfo == null)
        {
            throw new ArgumentException($"User info id not found #{userId}");
        }

        //Update fields
        account.Username = userUpdateDto.Username;
        userInfo.FirstName = userUpdateDto.FirstName;
        userInfo.LastName = userUpdateDto.LastName;
        userInfo.DateOfBirth = userUpdateDto.DateOfBirth;
        userInfo.Gender = userUpdateDto.Gender;
        userInfo.Phone = userUpdateDto.Phone;

        Account? updatedAcc = await _unitOfWork.Accounts.Update(account);
        UserInfo? updatedUserInfo = await _unitOfWork.UserInfos.Update(userInfo);
        await _unitOfWork.SaveChangeAsync();

        //mapper
        UserResponse userResponse = _mapper.Map<UserResponse>(updatedAcc);
        return userResponse;
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        try
        {
            Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == id);
            if (account == null)
                return false;
            bool isDelete = await _unitOfWork.Accounts.Delete(account);
            await _unitOfWork.SaveChangeAsync();
            if (isDelete)
            {
                Dictionary<string, object> headers = new Dictionary<string, object>()
                {
                    {"event", "user.delete" },
                    {"rowCount", 1 }
                };
                UserDeletionMessage userDeletionMessage = new UserDeletionMessage(account.UserID, account.Username);

                //ktra redis

                _publisher.Publish<UserDeletionMessage>(headers, userDeletionMessage);
            }
            return isDelete;
        }
        catch (Exception)
        {
            return false;
            throw;
        }
    }

    public async Task<UserResponse> UploadUserAvatar(Guid userId, IFormFile imageFile)
    {
        
        Account? account = await _unitOfWork.Accounts.GetByCondition(a => a.UserID == userId);
        if (account == null)
        {
            throw new NotFoundException($"User id not found #{userId}");
        }

        UserInfo? userInfo = await _unitOfWork.UserInfos.GetByCondition(a => a.UserID == userId);
        if(userInfo == null) {
            throw new NotFoundException($"User info id not found #{userId}");
        }
        string imageUrl = await _cloudinaryService.UploadImageAsync(imageFile, $"droniverse/users/avatars/{userId}");
        userInfo.ImageUrl = imageUrl;

        UserInfo? updatedUserInfo = await _unitOfWork.UserInfos.Update(userInfo);
        Account? updatedAccount = await _unitOfWork.Accounts.Update(account);
        await _unitOfWork.SaveChangeAsync();

        UserResponse userResponse = _mapper.Map<UserResponse>(updatedAccount);

        return userResponse;
    }

    public async Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds)
    {
        if (userIds == null || !userIds.Any())
            return [];

        IEnumerable<UserResponse> userResponses = await _unitOfWork.Accounts.GetUsersByIdsAsync(userIds);

        // 1. Tạo ra một tập hợp các Tasks (IEnumerable<Task<UserResponse>>)
        var userTasks = userResponses.Select(async userResponse =>
        {
            IEnumerable<UserLevelResponseDto>? userLevel = null;
            IEnumerable<UserLevelResponseDto>? userLevelMax = null;
            try
            {
                userLevel = await _academyMicroserviceClient.GetUserLevelsAsync(userResponse.UserId);
                userLevelMax = await _academyMicroserviceClient.GetUserLevelMaxAsync(userResponse.UserId);
            }
            catch (Exception ex)
            {
                // Sửa lại thành Structured Logging thay vì string interpolation ($"")
                _logger.LogWarning(ex, "Failed to get user level from Academy service for user {UserId}", userResponse.UserId);
            }

            return userResponse with { UserLevel = userLevel, UserLevelMax = userLevelMax };
        });

        // 2. Await tất cả các task cùng chạy song song và lấy kết quả trả về
        UserResponse[] updatedUserResponses = await Task.WhenAll(userTasks);

        // 3. Return danh sách đã được cập nhật
        return updatedUserResponses;
    }

    public async Task<IEnumerable<Guid>> GetUsersByUserInfo(UserInfoSearchRequest request)
    {
        var sortDirection = request?.SortDirection ?? SortDirection.Asc;
        var userIds = await _unitOfWork.Accounts.GetUsersByUserInfoAsync(
            request?.SearchName,
            sortDirection);

        return userIds;
    }

    public async Task<SearchUsersWithPaginationResponse> SearchUsersWithPagination(
      SearchUsersWithPaginationRequest request)
    {
        request ??= new SearchUsersWithPaginationRequest();

        var pageIndex = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var (totalItems, users) = await _unitOfWork.UserInfos
              .SearchUsersWithPaginationAsync(
                  request.FullName,
                  request.Email,
                  request.UserIds,
                  pageIndex,
                  pageSize);

        return new SearchUsersWithPaginationResponse
        {
            TotalItems = totalItems,
            Items = users
        };
    }

    public async Task<IEnumerable<SimpleUserReponse>> GetUsersByRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty", nameof(roleName));

        // Query accounts by role name
        IEnumerable<Account> accounts = await _unitOfWork.Accounts
            .GetManyByCondition(a => a.Role.RoleName == roleName);

        if (!accounts.Any())
            return [];

        // Get all user IDs to batch fetch user info
        var userIds = accounts.Select(a => a.UserID).ToList();
        var userInfos = await _unitOfWork.UserInfos
            .GetManyByCondition(u => userIds.Contains(u.UserID));

        // Build a dictionary for quick lookup
        var userInfoDict = userInfos.ToDictionary(u => u.UserID);

        // Map to SimpleUserReponse
        var simpleUsers = accounts.Select(account =>
        {
            userInfoDict.TryGetValue(account.UserID, out var userInfo);

            return new SimpleUserReponse
            {
                UserId = account.UserID,
                FullName = string.IsNullOrWhiteSpace(userInfo?.FirstName) && string.IsNullOrWhiteSpace(userInfo?.LastName)
                    ? account.Username
                    : $"{userInfo?.FirstName} {userInfo?.LastName}".Trim(),
                Email = account.Email,
                AvatarUrl = userInfo?.ImageUrl
            };
        }).ToList();

        return simpleUsers;
    }
}

