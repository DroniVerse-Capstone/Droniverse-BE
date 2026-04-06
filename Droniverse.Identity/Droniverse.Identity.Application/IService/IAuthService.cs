using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.IService;
public interface IAuthService
{
    Task<bool> Logout();
    Task<AuthResponse> RefreshToken(string accessToken, string refreshToken);
    Task<AuthResponse> AuthenticatedUser(LoginEmailDto loginEmailDto);
    Task<AuthResponse> RegisterUser(RegisterDto registerDto);
    Task<UserResponse?> GetCurrentUserInfo();
    Task<UserResponse?> UpdateProfileAsync(ProfileUpdateDto userUpdateDto);
}

