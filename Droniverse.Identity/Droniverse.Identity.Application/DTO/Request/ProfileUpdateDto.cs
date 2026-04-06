using Microsoft.AspNetCore.Http;

namespace Droniverse.Identity.Application.DTO.Request;

public record ProfileUpdateDto
(
    string Username,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
){}

