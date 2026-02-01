namespace Droniverse.Community.Application.DTO.Response;
public record UserResponse(
    Guid UserId,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    DateTime? DateOfBirth,
    string RoleName
    )
{
    public UserResponse() : this(default, default, default, default, default, default, default)
    {
    }
}
