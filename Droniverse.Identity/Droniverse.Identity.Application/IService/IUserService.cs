using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;

namespace Droniverse.Identity.Application.IService;
public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsers();
    Task<UserResponse> AddUser(UserCreateDto userCreateDto);
    Task<UserResponse> UpdateUser(Guid userId, UserUpdateDto userUpdateDto);
    Task<UserResponse> GetUserById(Guid id);
    Task<bool> DeleteUser(Guid id);
}

