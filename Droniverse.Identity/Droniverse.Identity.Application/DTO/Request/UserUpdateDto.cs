using Microsoft.AspNetCore.Http;

namespace Droniverse.Identity.Application.DTO.Request;
public record UserUpdateDto(
    Guid UserId,
    string Username,
    //string PasswordHash,
    //string Email,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    IFormFile ImageFile
    //Guid RoleId
    )
{
}

