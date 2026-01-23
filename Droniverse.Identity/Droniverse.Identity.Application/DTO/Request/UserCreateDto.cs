namespace Droniverse.Identity.Application.DTO.Request;
public record UserCreateDto(
    string Username,
    string PasswordHash,
    string Email,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Guid RoleId
)
{}

