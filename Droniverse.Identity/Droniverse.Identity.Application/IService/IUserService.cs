using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace Droniverse.Identity.Application.IService;
public interface IUserService
{
    Task<UserDashboardSummaryResponse> GetUserDashboardSummary(string filterTimeLine = "month");
    Task<IEnumerable<UserTimelineOptionResponse>> GetFilterTimeLineOptions();
    Task<IEnumerable<UserResponse>> GetNewUsersByTimeline(string filterTimeLine);
    Task<PaginationResult<IEnumerable<UserResponse>>> GetAllUsers(
        UserSearchRequest userSearchRequest,
        int pageIndex,
        int pageSize);

    Task<UserResponse> AddUser(UserCreateDto userCreateDto);
    Task<UserResponse> UploadUserAvatar(Guid userId, IFormFile imageFile);
    Task<UserResponse> UpdateUser(Guid userId, UserUpdateDto userUpdateDto);
    Task<UserResponse> GetUserById(Guid id);
    Task<bool> DeleteUser(Guid id);
    Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds);
    Task<IEnumerable<Guid>> GetUsersByUserInfo(UserInfoSearchRequest request);
    Task<SearchUsersWithPaginationResponse> SearchUsersWithPagination(SearchUsersWithPaginationRequest request);
    Task<IEnumerable<SimpleUserReponse>> GetUsersByRole(string roleName);
}

