using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Shared.DTOs.Response;
public record UserResponse(
    Guid UserId,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    DateTime? DateOfBirth,
    string RoleName,
    string? ImageUrl,
    GenderOptions Gender,
    LevelMiniResponseDto? Level
    )
{
    public UserResponse() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, default, default)
    {
    }
}
