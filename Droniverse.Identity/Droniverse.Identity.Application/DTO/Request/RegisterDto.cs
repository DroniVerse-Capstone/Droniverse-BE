namespace Droniverse.Identity.Application.DTO.Request;
public record RegisterDto(
    string Password,
    string Email,
    string FirstName,
    string LastName,
    string RoleName
)
{}

