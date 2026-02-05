using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.DTO.Response;
public record AuthResponse(string AccessToken, string RefreshToken, UserResponse User)
{
    public AuthResponse() : this(string.Empty, string.Empty, new UserResponse()) { }
}
