using System.ComponentModel.DataAnnotations;

namespace Droniverse.Community.Application.DTO.Request
{
    public record ClubJoinDto(

        [Length(6, 6, ErrorMessage = "Club code must be 6 characters !")]
        string clubCode,

        Guid? mediaID,

        string? clubRequirement
        )
    { }
}
