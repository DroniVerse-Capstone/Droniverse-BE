using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Shared.DTOs;

public record GetParticipantsResponseDTO
{
    public Guid UserId { get; init; }
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? ImageUrl { get; init; }
    public GenderOptions Gender { get; init; }
    public DateTime JoinDate { get; init; }
}
