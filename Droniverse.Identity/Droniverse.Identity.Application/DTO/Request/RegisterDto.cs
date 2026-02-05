namespace Droniverse.Identity.Application.DTO.Request;
public record RegisterDto(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? Phone
)
{}

