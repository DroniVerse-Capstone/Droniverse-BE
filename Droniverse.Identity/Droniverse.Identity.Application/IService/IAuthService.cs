using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.IService;
public interface IAuthService
{
    Task<AuthResponse> AuthenticatedUser(string email, string password);
    Task<AuthResponse> RegisterUser(RegisterDto registerDto);
}

