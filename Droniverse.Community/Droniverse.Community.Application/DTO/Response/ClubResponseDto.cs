using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;
using System.Text.Json.Serialization;

namespace Droniverse.Community.Application.DTO.Response;

public record ClubResponseDto
{
    public Guid ClubID { get; init; }
    public required string NameVN { get; init; }
    public required string NameEN { get; init; }
    public required string DescriptionVN { get; init; }
    public required string DescriptionEN { get; init; }
    public required string ClubCode { get; init; }
    public ClubStatus Status { get; init; }
    public bool IsPublic { get; init; }
    public string? ImageUrl { get; init; }
    public int LimitParticipation { get; init; }
    public int LimitClubManagers { get; init; }
    public int TotalMembers { get; set; }
    public int TotalCourses { get; set; }
    public string? SuspendedReason { get; set; }
    public DroneResponseDto Drone { get; set; }
    public UserResponse? Creator { get; set; }
}