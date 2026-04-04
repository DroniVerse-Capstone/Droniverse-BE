using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace Droniverse.Identity.Application.IService;
public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsers();
    Task<UserResponse> AddUser(UserCreateDto userCreateDto);
    Task<string> UploadUserAvatar(Guid userId, IFormFile imageFile);
    Task<UserResponse> UpdateUser(Guid userId, UserUpdateDto userUpdateDto);
    Task<UserResponse> GetUserById(Guid id);
    Task<bool> DeleteUser(Guid id);
    Task<IEnumerable<UserResponse>> GetUsersByIds(IEnumerable<Guid> userIds);
    Task<IEnumerable<SimpleUserReponse>> GetUsersByUserInfo(UserInfoSearchRequest request);
}

